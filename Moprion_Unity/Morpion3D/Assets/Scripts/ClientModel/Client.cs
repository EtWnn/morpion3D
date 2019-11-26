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
    /// <summary>
    /// handles the TCP communications between the application and the server
    /// </summary>
    public class Client
    {

        // ---- Events ---

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler GameUpdated;
        public event EventHandler<MatchRequestEventArgs> MatchRequestUpdated;
        public event EventHandler<TEventArgs<List<User>>> OpponentListUpdated;
        public event EventHandler OpponentDisconnected;

        // ---- Static fields/properties ----

        private static Dictionary<NomCommande, Action<byte[], Client>> methods = new Dictionary<NomCommande, Action<byte[], Client>>();

        // ---- Public fields/properties ----

        public readonly string LogFile;
        public Int32 Port = 13000;
        public IPAddress Ip = IPAddress.Parse("127.0.0.1");
        public NetworkStream Stream = null;
        public bool is_connected = false;

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


        // ---- Private fields/properties ----

        private Socket socket = null;
        private IPEndPoint remoteEP = null;

        private bool continueListening = false;
        private Thread listeningThread = null;
        private Thread pingThread = null;

        private readonly object streamLock = new object();
        private readonly object usersLock = new object();

        private const int pingDelay = 500;

        // ---- Static methods ----

        public static void InnitMethods()
        {
            methods[NomCommande.OUS] = Messaging.RecieveOtherUsers;
            methods[NomCommande.RGR] = Messaging.RecieveGameRequestStatus;
            methods[NomCommande.MRQ] = Messaging.RecieveGameRequest;
            methods[NomCommande.DGB] = Messaging.RecieveGameBoard;
            methods[NomCommande.NDC] = Messaging.RecieveOpponentDisconnection;
            methods[NomCommande.PNG] = Messaging.RecievePing;
            methods[NomCommande.MSG] = Messaging.RecieveMessage;
        }

        // ---- Public methods ----

        public Client()
        {
            string to_date_string = DateTime.Now.ToString("s");
            Directory.CreateDirectory("logs");
            LogFile = "logs/client_log_" + to_date_string + ".txt";
            LogFile = LogFile.Replace(':', '_');

            LogWriter = new LogWriter(LogFile);
        }

        /// <summary>
        /// try to connect the client to the server at: <see cref="Ip"/> and <see cref="Port"/>
        /// </summary>
        public void tryConnect()
        {
            if( this.socket == null || !this.socket.Connected)
            {
                this.remoteEP = new IPEndPoint(this.Ip, this.Port);
                this.socket = new Socket(this.Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    this.socket.Connect(this.remoteEP);
                    if (this.socket.Connected)
                    {
                        this.continueListening = true;

                        this.Stream = new NetworkStream(this.socket);
                        this.Stream.ReadTimeout = 100;
                        
                        this.listeningThread = new Thread(Listen);
                        this.listeningThread.IsBackground = true;
                        this.listeningThread.Start();
                        
                    	this.pingThread = new Thread(Ping);
                    	this.pingThread.IsBackground = true;
                    	this.pingThread.Start();

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

        /// <summary>
        /// try to disconnect the client from the server
        /// </summary>
        public void tryDisconnect()
        {
            if (this.socket != null && this.socket.Connected)
            {
                Debug.Log("Disconnecting...");
                this.continueListening = false;
                this.socket.Close();

                is_connected = false;
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// read thread-safely the server stream 
        /// </summary>
        /// <param name="message"></param>
        /// <returns> the number of bytes read</returns>
        public int StreamRead(byte[] message)
        {
            int n_bytes = 0;
            lock (streamLock)
            {
                n_bytes = this.Stream.Read(message, 0, message.Length);
            }
            return n_bytes;
        }

        /// <summary>
        /// write thread-safely on the server stream 
        /// </summary>
        /// <param name="message"></param>
        public void StreamWrite(byte[] message)
        {
            lock (streamLock)
            {
                this.Stream.Write(message, 0, message.Length);
            }
        }

        /// <summary>
        /// trigger the function <see cref="Messaging.AskOtherUsers"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnMatchUpdatingOpponentList(object sender, EventArgs e)
        {
            Messaging.AskOtherUsers(this);
        }

        /// <summary>
        /// trigger communications functions according to the <see cref="MatchRequestEventArgs.EStatus"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// trigger the function <see cref="Messaging.SendPositionPlayer"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPositionPlayed(object sender, TEventArgs<System.Numerics.Vector3> e)
        {
            var vec = e.Data;
            Messaging.SendPositionPlayer(this, vec);
        }

        /// <summary>
        /// disconnect and reconnect to the server according to the new Ip/Port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnServerInfoUpdated(object sender, ServerInfoEventArgs e)
        {
            tryDisconnect();
            Ip = IPAddress.Parse(e.IP);
            Port = int.Parse(e.Port);
            Debug.Log($"New server address: {Ip}:{Port}");
            tryConnect();
        }

        /// <summary>
        /// trigger the function <see cref="Messaging.SendUserName"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUsernameUpdate(object sender, UsernameEventArgs e)
        {
            Messaging.SendUserName(this, e.Username);
        }

        // ---- Internal methods ----

        /// <summary>
        /// invoke <see cref="OpponentDisconnected"/>
        /// </summary>
        internal void RaiseOpponentDisconnected()
        {
            this.GameClient = null;
            OpponentDisconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// invoke <see cref="MatchRequestUpdated"/>
        /// </summary>
        /// <param name="matchRequestEventArgs"></param>
        internal void RaiseMatchRequestUpdated(MatchRequestEventArgs matchRequestEventArgs)
        {
            var handler = MatchRequestUpdated;
            if (handler != null)
                handler(this, matchRequestEventArgs);
        }

        /// <summary>
        /// invoke  <see cref="OpponentListUpdated"/>
        /// </summary>
        /// <param name="listUsers"></param>
        internal void RaiseOpponentListUpdated(List<User> listUsers)
        {
            var handler = OpponentListUpdated;
            if (handler != null)
                handler(this, new TEventArgs<List<User>>(listUsers));
        }

        // ---- Private methods ----

        /// <summary>
        /// Send a ping to the server every <see cref="pingDelay"/> seconds
        /// </summary>
        private void Ping()
        {
            while (this.continueListening)
            {
                try
                {
                    Messaging.SendPing(this);
                }
                catch (Exception)
                {
                    Debug.Log("try disconnect with ping method");
                    this.continueListening = false;
                    Debug.Log("this._socket.Connected : " + this.socket.Connected);
                    tryDisconnect();
                }
                Thread.Sleep(pingDelay);
            }
        }           

        /// <summary>
        /// listen continuously to the stream to catch what the server sends
        /// </summary>
        private void Listen()
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
                            LogWriter.Write($"CMD ERROR, CMD: {cmd}, following_length: {following_length}, EX:{ex}");
                            this.Stream.Flush();
                        }
                    }

                    Thread.Sleep(10);

                }
                catch (Exception ex)
                {
                    this.continueListening = false;
                    LogWriter.Write($"ERROR: Listen crashed:  {ex}");
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


        
        
    }
}
