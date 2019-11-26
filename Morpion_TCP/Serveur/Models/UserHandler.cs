using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serveur.Models;
using Serveur.Functions;

namespace Serveur.Models
{
    /// <summary>
    /// Handle one client for the server
    /// </summary>
    public class UserHandler
    {

        // ---- Static fields/properties ----

        private static Dictionary<NomCommande, Func<byte[], UserHandler, byte[]>> methods = new Dictionary<NomCommande, Func<byte[], UserHandler, byte[]>>();

        private readonly Server Server;

        private TcpClient ClientSocket { get; set; }
        private NetworkStream Stream;
        private readonly object streamLock = new object();

        private const int pingDelay = 500;

        // ---- Public fields/properties ----

        public bool KeepChatting;
        public int Id { get; private set; }
        public string UserName { get; set; }

        public ModelGame.Game Game { get; set; }

        public LogWriter ServerLogWriter { get => Server.LogWriter; }
        public Dictionary<int, UserHandler> UsersHandlers { get => Server.UsersHandlers; }

        public bool Connected => ClientSocket.Connected;

        // ---- Private fields/properties ----


        // ---- Static methods ----

        public static void InnitMethods()
        {
            methods[NomCommande.MSG] = Messaging.RecieveMessage;
            methods[NomCommande.USN] = Messaging.RecieveUserName;
            methods[NomCommande.OUS] = Messaging.SendOtherUsers;
            methods[NomCommande.NPP] = Messaging.ReceivePositionPlayed;
            methods[NomCommande.DGB] = Messaging.SendGameBoard;
            methods[NomCommande.MRQ] = Messaging.TransferMatchRequest;
            methods[NomCommande.GRR] = Messaging.TransferGameRequestResponse;
            methods[NomCommande.PNG] = Messaging.RecievePing;
        }

        // ---- Public methods ----

        public UserHandler(TcpClient inClientSocket, int id, Server server)
        {
            ClientSocket = inClientSocket;
            UserName = "default_" + id.ToString();
            Id = id;
            Game = null;
            Server = server;

            this.KeepChatting = true;
        }


        /// <summary>
        /// launch in threads the methods <see cref="DoChat"/> and <see cref="Ping"/> for the communication with the client
        /// </summary>
        public void Start()
        {
            Thread ctThread = new Thread(DoChat);
            Thread pingThread = new Thread(Ping);

            ctThread.Start();
            pingThread.Start();
        }


        /// <summary>
        /// read thread-safely the client stream 
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
        /// write thread-safely on the client stream 
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
        /// serialize the <see cref="Id"/> and the <see cref="UserName"/> in a bytes array
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            var id_bytes = BitConverter.GetBytes((Int16)this.Id);
            var username_length_bytes = BitConverter.GetBytes((Int16)this.UserName.Length);
            var username_bytes = Encoding.UTF8.GetBytes(this.UserName);

            byte[] bytes = new byte[id_bytes.Length + username_length_bytes.Length + username_bytes.Length];
            id_bytes.CopyTo(bytes, 0);
            username_length_bytes.CopyTo(bytes, id_bytes.Length);
            username_bytes.CopyTo(bytes, id_bytes.Length + username_length_bytes.Length);

            return bytes;
        }

        // ---- Private methods ----

        /// <summary>
        /// Send a ping to the server every <see cref="pingDelay"/> seconds
        /// </summary>
        private void Ping()
        {
            Thread.Sleep(2000);
            while (KeepChatting)
            {
                try
                {
                    Messaging.SendPing(this);
                }
                catch (Exception) //à faire: prendre en compte la fermeture innatendue du canal par le serveur
                {

                    ServerLogWriter.Write($"try disconnecting id {Id} with ping method");
                    KeepChatting = false;
                    if (Game != null) //si le joueur était en jeu
                    {
                        Messaging.SendNotifcationDisconnection(this);
                        Game = null;
                    }
                    ClientSocket.Close();
                }
                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// recieve the commands from the client and answer accordingly
        /// </summary>
        private void DoChat()
        {
            lock(streamLock)
            {
                Stream = this.ClientSocket.GetStream();
            }
            Messaging.SendMessage(this, "Hi new user! You have been assigned the id " + this.Id.ToString() );

            while (KeepChatting)
            {
                
                try
                {
                    Byte[] bytes = new Byte[5];
                    int n_bytes = StreamRead(bytes);
                    if (n_bytes >= 5) //minimum number of bytes for CMD + length to follow
                    {
                        
                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        if(cmd != "PNG")
                        {
                            Server.LogWriter.Write($"command recieved from client {this.UserName} Id {this.Id} : {cmd} de taille {following_length} {n_bytes}");
                        }
                        byte[] following_bytes = new byte[following_length];
                        if(following_length > 0)
                        {
                            StreamRead(following_bytes);
                        }

                        byte[] response = UserHandler.methods[(NomCommande)Enum.Parse(typeof(NomCommande), cmd)](following_bytes, this);

                        if (response.Length > 0)
                        {
                            StreamWrite(response);
                        }
                    }
                    

                    
                }
                catch (System.IO.IOException ex) //à faire: prendre en compte la fermeture inatendue du canal par le client
                {
                    KeepChatting = false;
                    Server.LogWriter.Write($"the user {this.UserName} Id {this.Id} got disconnected");

                    if(Game != null) //si le joueur était en jeu
                    {
                        int IdPlayer1 = Game.IdPlayer1;
                        int IdPlayer2 = Game.IdPlayer2;
                        Messaging.SendNotifcationDisconnection(this);

                        Server.UsersHandlers[IdPlayer1].Game = null;
                        Server.UsersHandlers[IdPlayer2].Game = null;
                    }

                    this.ClientSocket.Close();
                }
            }

        }
        
    }
}
