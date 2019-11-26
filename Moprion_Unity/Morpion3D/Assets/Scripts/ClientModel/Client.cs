using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MyClient.Functions;
using MyClient.Models;
using MyClient.ModelGame;
using UnityEngine;

namespace MyClient
{
    public class Client
    {
        public Mutex LogMutex = new Mutex();
        public readonly string LogFile;
        public Int32 port = 13000;
        public IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        private Socket _socket = null;
        private IPEndPoint _remoteEP = null;
        private bool _continueListen = false;
        private Thread _listeningThread = null;
        private Thread _pingThread = null;
        public NetworkStream Stream = null;
        public Mutex StreamMutex = new Mutex();

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler GameUpdated;
        public event EventHandler<MatchRequestEventArgs> MatchRequestUpdated;
        public event EventHandler<TEventArgs<List<User>>> OpponentListUpdated;
        public event EventHandler OpponentDisconnected;


        public bool is_connected = false;


        public Mutex mutex = new Mutex();
        public Dictionary<int, User> connected_users = new Dictionary<int, User>();


        public Dictionary<int, User> gameRequestsRecieved = new Dictionary<int, User>();
        public User Opponent = null;
        
        private Game _GameClient = null;
        public Game GameClient
        {
            get => _GameClient;
            set
            {
                _GameClient = value;
                GameUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private static Dictionary<NomCommande, Action<byte[], Client>> methods = new Dictionary<NomCommande, Action<byte[], Client>>();
        public static void InnitMethods()
        {
            methods[NomCommande.OUS] = Messaging.RecieveOtherUsers;
            methods[NomCommande.RGR] = Messaging.RecieveGameRequestStatus;
            methods[NomCommande.MRQ] = Messaging.RecieveGameRequest;
            methods[NomCommande.DGB] = Messaging.RecieveGameBoard;
            methods[NomCommande.NDC] = Messaging.RecieveOpponentDisconnection;
        }

        public Client()
        {
            string to_date_string = DateTime.Now.ToString("s");
            LogFile = "client_log_" + to_date_string + ".txt";
            LogFile = LogFile.Replace(':', '_');
        }

        public void tryConnect()
        {
            if( this._socket == null || !this._socket.Connected)
            {
                this._remoteEP = new IPEndPoint(this.localAddr, this.port);
                this._socket = new Socket(this.localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //connection to the server
                    this._socket.Connect(this._remoteEP);
                    if (this._socket.Connected)
                    {
                        this._continueListen = true;

                        this.Stream = new NetworkStream(this._socket);
                        this.Stream.ReadTimeout = 10;

                        //launching the listening thread
                        this._listeningThread = new Thread(Listen);
                        this._listeningThread.IsBackground = true;
                        this._listeningThread.Start();

                        //launching the ping thread
                    	this._pingThread = new Thread(Ping);
                    	this._pingThread.IsBackground = true;
                    	this._pingThread.Start();

                        is_connected = true;
                        Debug.Log("Before Connected event");
                        Connected?.Invoke(this, EventArgs.Empty);
                        Debug.Log("After Connected event");
                    }
                }
                catch (SocketException)
                {
                }
            }
        }

        ~Client()
        {
            if (this._socket == null || !this._socket.Connected)
            {
                this._socket.Close();
            }
        }

        public void tryDisconnect()
        {
            if (this._socket != null && this._socket.Connected)
            {
                this._continueListen = false;
                this._socket.Close();

                is_connected = false;
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        void Ping()
        {
            while (this._continueListen)
            {
                try
                {
                    Messaging.SendPing(this);
                }
                catch (Exception) //à faire: prendre en compte la fermeture innatendue du canal par le serveur
                {
                    this.StreamMutex.ReleaseMutex();
                    Debug.Log("try disconnect with ping method");
                    this._continueListen = false;
                    Debug.Log("this._socket.Connected : " + this._socket.Connected);
                    tryDisconnect();
                }
                Thread.Sleep(1000);
            }
        }           

        void Listen()
        {
            while (this._continueListen)
            {

                try
                {
                    Byte[] bytes = new Byte[5];
                    int n_bytes = 0;
                    try
                    {
                        n_bytes = Messaging.StreamRead(this, bytes);
                    }
                    catch (IOException)
                    {
                        n_bytes = 0;
                        this.StreamMutex.ReleaseMutex();
                    }
                    
                    if (n_bytes >= 5) //minimum number of bytes for CMD + length to follow
                    {

                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        byte[] following_bytes = new byte[following_length];
                        if (following_length > 0)
                        {
                            Messaging.StreamRead(this, following_bytes);
                        }

                        try
                        {
                            NomCommande cmd_type = (NomCommande)Enum.Parse(typeof(NomCommande), cmd);
                            Messaging.WriteLog(this, $"command recieved: {cmd}, following_length: {following_length}");
                            Client.methods[cmd_type](following_bytes, this);

                        }
                        catch (Exception ex)
                        {
                            //write_in_log
                            Messaging.WriteLog(this, $"CMD ERROR, CMD: {cmd}, following_length: {following_length}, EX:{ex}");
                            this.Stream.Flush();
                        }
                    }

                    Thread.Sleep(100);

                }
                catch (Exception ex) //à faire: prendre en compte la fermeture innatendue du canal par le serveur
                {
                    this._continueListen = false;
                    Messaging.WriteLog(this, $"ERROR: Listen crashed:  {ex}");
                }
            }
        }
        
        internal void RaiseOpponentDisconnected()
        {
            //this.GameClient = null;
            OpponentDisconnected?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseMatchRequestUpdated(MatchRequestEventArgs matchRequestEventArgs)
        {
            var handler = MatchRequestUpdated;
            if (handler != null)
                handler(this, matchRequestEventArgs);
        }

        internal void RaiseOpponentListUpdated(List<User> listUsers)
        {
            var handler = OpponentListUpdated;
            if (handler != null)
                handler(this, new TEventArgs<List<User>>(listUsers));
        }

        public void OnMatchUpdatingOpponentList(object sender, EventArgs e)
        {
            Messaging.AskOtherUsers(this);
        }

        public void OnMatchRequestUpdated(object sender, MatchRequestEventArgs e)
        {
            switch (e.Status)
            {
                case MatchRequestEventArgs.EStatus.New:
                    Messaging.RequestMatch(this, e.User.Id);
                    break;
                case MatchRequestEventArgs.EStatus.Canceled:
                    // a coder nice to have
                    break;
                case MatchRequestEventArgs.EStatus.Accepted:
                    Messaging.SendGameRequestResponse(this, e.User.Id, true);
                    break;
                case MatchRequestEventArgs.EStatus.Declined:
                    Messaging.SendGameRequestResponse(this, e.User.Id, false);
                    break;
                case MatchRequestEventArgs.EStatus.CannotBeReached:
                    // nice to have
                    break;
                default:
                    break;
            }

        }

        public void OnPositionPlayed(object sender, TEventArgs<System.Numerics.Vector3> e)
        {
            var vec = e.Data;
            Messaging.SendPositionPlayer(this, vec);
        }
    }
}
