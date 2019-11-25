using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serveur.Functions;

namespace Serveur.Models
{
    public class UserHandler
    {
        private static Dictionary<NomCommande, Func<byte[], UserHandler, byte[]>> methods = new Dictionary<NomCommande, Func<byte[], UserHandler, byte[]>>();
        public static void InnitMethods()
        {
            methods[NomCommande.MSG] = Messaging.RecieveMessage;
            methods[NomCommande.USN] = Messaging.RecieveUserName;
            methods[NomCommande.OUS] = Messaging.SendOtherUsers;
            methods[NomCommande.NPP] = Messaging.ReceivePositionPlayed;
            methods[NomCommande.DGB] = Messaging.SendGameBoard;
            methods[NomCommande.MRQ] = Messaging.TransferMatchRequest;
            methods[NomCommande.GRR] = Messaging.TransferGameRequestResponse;
        }


        private Mutex usersMutex = new Mutex();
        private Dictionary<int, UserHandler> userHandlers;
        public Dictionary<int, UserHandler> UsersHandlers
        {
            set
            {
                usersMutex.WaitOne();
                userHandlers = value;
                usersMutex.ReleaseMutex();
            }
            get
            {
                usersMutex.WaitOne();
                Dictionary<int, UserHandler> userHandlers_copy = new Dictionary<int, UserHandler>(userHandlers);
                usersMutex.ReleaseMutex();
                return userHandlers_copy;
                
            }
        }





        public bool KeepChatting;
        public int Id { get; private set; }
        public string UserName { get; set; }
        public NetworkStream stream;
        public TcpClient clientSocket { get; set; }
        public ModelGame.Game Game { get; set; }
        public readonly string log_file;

        public UserHandler(TcpClient inClientSocket, int id, Dictionary<int, UserHandler> userHandlers, Mutex usersMutex, string log_file)
        {
            this.clientSocket = inClientSocket;
            this.UserName = "default_" + id.ToString();
            this.Id = id;
            this.Game = null;
            this.UsersHandlers = userHandlers;
            this.usersMutex = usersMutex;
            this.log_file = log_file;
            this.KeepChatting = true;


        }

        public void Start()
        {
            Thread ctThread = new Thread(DoChat);
            ctThread.Start();
        }

        private void DoChat()
        {
            stream = this.clientSocket.GetStream();
            Messaging.SendMessage(stream, "Hi new user! You have been assigned the id " + this.Id.ToString() );

            while (KeepChatting)
            {
                
                try
                {
                    Byte[] bytes = new Byte[5];
                    int NombreOctets = stream.Read(bytes, 0, bytes.Length);
                    if(NombreOctets >= 5) //minimum number of bytes for CMD + length to follow
                    {
                        
                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        Messaging.WriteLog(log_file, $"command recieved from client {this.UserName} Id {this.Id} : {cmd} de taille {following_length} {NombreOctets}");
                        byte[] following_bytes = new byte[following_length];
                        if(following_length > 0)
                        {
                            stream.Read(following_bytes, 0, following_bytes.Length);
                        }

                        byte[] response = UserHandler.methods[(NomCommande)Enum.Parse(typeof(NomCommande), cmd)](following_bytes, this);

                        if(response.Length > 0)
                        {
                            stream.Write(response, 0, response.Length);
                        }
                    }
                    

                    
                }
                catch (System.IO.IOException ex) //à faire: prendre en compte la fermeture inatendue du canal par le client
                {
                    KeepChatting = false;
                    Messaging.WriteLog(log_file, $"the user {this.UserName} Id {this.Id} got disconnected");

                    if(Game != null) //si le joueur était en jeu
                    {
                        // à faire: prévenir l'autre joueur

                        Game = null;
                    }

                    this.clientSocket.Close();
                }
            }

        }

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

        public bool IsAlive()
        {
            try
            {
                byte[] test = new byte[1];
                this.stream.Write(test, 0, test.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
