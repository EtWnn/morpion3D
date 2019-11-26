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
        public readonly string LogFile;
        public Int32 Port = 13000;
        public IPAddress Ip = IPAddress.Parse("127.0.0.1");
        public int TryConnectPeriodMs = 100; 

        private Socket socket = null;
        private IPEndPoint remoteEP = null;
        private bool continueListening = false;
        private Thread listeningThread = null;
        private Thread pingThread = null;
        private Thread tryConnectThread = null;

        private readonly object streamLock;
        public NetworkStream Stream = null;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler GameUpdated;
        public event EventHandler<MatchRequestEventArgs> MatchRequestUpdated;
        public event EventHandler<TEventArgs<List<User>>> OpponentListUpdated;
        public event EventHandler OpponentDisconnected;


        public bool is_connected = false;


        private readonly object usersLock = new object();
        private Dictionary<int, User> connectedUsers = new Dictionary<int, User>();
        public Dictionary<int, User> ConnectedUsers
        {
            set
            {
                lock (usersLock)
                {
                    connectedUsers = value;
                }
            }
            get
            {
                Dictionary<int, User> connectedUsers_copy;
                lock (usersLock)
                {
                    connectedUsers_copy = new Dictionary<int, User>(connectedUsers);
                }
                return connectedUsers_copy;

            }
        }

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

        public readonly LogWriter LogWriter;

        private static Dictionary<NomCommande, Action<byte[], Client>> methods = new Dictionary<NomCommande, Action<byte[], Client>>();
        public static void InnitMethods()
        {
            methods[NomCommande.OUS] = Messaging.RecieveOtherUsers;
            methods[NomCommande.RGR] = Messaging.RecieveGameRequestStatus;
            methods[NomCommande.MRQ] = Messaging.RecieveGameRequest;
            methods[NomCommande.DGB] = Messaging.RecieveGameBoard;
            methods[NomCommande.NDC] = Messaging.RecieveOpponentDisconnection;
            methods[NomCommande.PNG] = Messaging.RecievePing;
        }

        public Client()
        {
            string to_date_string = DateTime.Now.ToString("s");
            Directory.CreateDirectory("logs");
            LogFile = "logs/client_log_" + to_date_string + ".txt";
            LogFile = LogFile.Replace(':', '_');

            LogWriter = new LogWriter(LogFile);
            streamLock = new object();
        }

        public void RepeatTryConnect()
        {
            tryConnectThread = new Thread(() =>
            {
                while(!is_connected)
                {
                    tryConnect();
                    Thread.Sleep(TryConnectPeriodMs);
                }
            });
            tryConnectThread.Start();
        }
        
        public void tryConnect()
        {
            if( this.socket == null || !this.socket.Connected)
            {
                this.remoteEP = new IPEndPoint(this.Ip, this.Port);
                this.socket = new Socket(this.Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //connection to the server
                    this.socket.Connect(this.remoteEP);
                    if (this.socket.Connected)
                    {
                        this.continueListening = true;

                        this.Stream = new NetworkStream(this.socket);
                        this.Stream.ReadTimeout = 10;

                        //launching the listening thread
                        this.listeningThread = new Thread(Listen);
                        this.listeningThread.IsBackground = true;
                        this.listeningThread.Start();

                        //launching the ping thread
                    	this.pingThread = new Thread(Ping);
                    	this.pingThread.IsBackground = true;
                    	this.pingThread.Start();

                        is_connected = true;
                        Connected?.Invoke(this, EventArgs.Empty);
                        Debug.Log($"After connection {this.socket.Connected}");
                    }
                }
                catch (SocketException)
                {
                }
            }
        }

        ~Client()
        {
            if (this.socket == null || !this.socket.Connected)
            {
                this.socket.Close();
            }
        }

        public void tryDisconnect()
        {
            if (this.socket != null)
            {
                Debug.Log("Disconnecting...");
                this.continueListening = false;
                this.socket.Close();

                is_connected = false;
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        void Ping()
        {
            while (this.continueListening)
            {
                try
                {
                    Messaging.SendPing(this);
                }
                catch (Exception) //à faire: prendre en compte la fermeture innatendue du canal par le serveur
                {
                    this.continueListening = false;
                    tryDisconnect();
                }
                Thread.Sleep(1000);
            }
        }           

        void Listen()
        {
            while (this.continueListening)
            {

                try
                {
                    Byte[] bytes = new Byte[5];
                    int n_bytes = 0;
                    try
                    {
                        n_bytes = StreamRead(bytes);
                    }
                    catch (IOException)
                    {
                        n_bytes = 0;
                    }
                    
                    if (n_bytes >= 5) //minimum number of bytes for CMD + length to follow
                    {

                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        byte[] following_bytes = new byte[following_length];
                        if (following_length > 0)
                        {
                            StreamRead(following_bytes);
                        }

                        try
                        {
                            NomCommande cmd_type = (NomCommande)Enum.Parse(typeof(NomCommande), cmd);
                            if(cmd != "PNG")
                            {
                                LogWriter.Write($"command recieved: {cmd}, following_length: {following_length}");
                            }
                            Client.methods[cmd_type](following_bytes, this);

                        }
                        catch (Exception ex)
                        {
                            //write_in_log
                            LogWriter.Write($"CMD ERROR, CMD: {cmd}, following_length: {following_length}, EX:{ex}");
                            this.Stream.Flush();
                        }
                    }

                    Thread.Sleep(10);

                }
                catch (Exception ex) //à faire: prendre en compte la fermeture innatendue du canal par le serveur
                {
                    this.continueListening = false;
                    LogWriter.Write($"ERROR: Listen crashed:  {ex}");
                }
            }
        }

        public int StreamRead(byte[] message)
        {
            int n_bytes = 0;
            lock (streamLock)
            {
                n_bytes = this.Stream.Read(message, 0, message.Length);
            }
            return n_bytes;
        }

        public void StreamWrite(byte[] message)
        {
            lock (streamLock)
            {
                this.Stream.Write(message, 0, message.Length);
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

        public void OnServerInfoUpdated(object sender, ServerInfoEventArgs e)
        {
            tryDisconnect();
            Ip = IPAddress.Parse(e.IP);
            Port = int.Parse(e.Port);
            Debug.Log($"New server address: {Ip}:{Port}");
            RepeatTryConnect();
        }

        public void OnUsernameUpdate(object sender, UsernameEventArgs e)
        {
            Messaging.SendUserName(this, e.Username);
        }
    }
}
