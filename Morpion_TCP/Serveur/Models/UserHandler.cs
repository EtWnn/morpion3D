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
        private static Mutex mutex = new Mutex();
        private static Dictionary<int, UserHandler> userHandlers = new Dictionary<int, UserHandler> { };
        public static Dictionary<int, UserHandler> UsersHandlers
        {
            get
            {
                mutex.WaitOne();
                Dictionary<int, UserHandler> userHandlers_copy = new Dictionary<int, UserHandler>(userHandlers);
                mutex.ReleaseMutex();
                return userHandlers_copy;
                
            }
        }
        private static int next_id = 0;

        private static Dictionary<NomCommande, Func<byte[], UserHandler, byte[]>> methods = new Dictionary<NomCommande, Func<byte[], UserHandler, byte[]>>();

        public static void InnitMethods()
        {
            methods[NomCommande.MSG] = Messaging.RecieveMessage;
            methods[NomCommande.USN] = Messaging.RecieveUserName;
            methods[NomCommande.OUS] = Messaging.SendOtherUsers;
            methods[NomCommande.NPP] = Messaging.ReceivePositionPlayed;
            methods[NomCommande.DGB] = Messaging.SendGameBoard;
        }


        public int Id { get; private set; }
        public string UserName { get; set; }
        public TcpClient clientSocket { get; set; }
        public ModelGame.Game Game { get; set; } 

        public UserHandler(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            this.UserName = "default_" + next_id.ToString();
            this.Id = next_id;
            this.Game = null;

            mutex.WaitOne();
            userHandlers[this.Id] = this; //on stocke le nouvel utilisateur dans le dictionnaire static users
            userHandlers[this.Id].Start(); //on lance le canal de communication avec le nouvel utilisateur
            mutex.ReleaseMutex();

            Console.WriteLine($" >> A new connexion has been made, the user has been asigned the id {this.Id}");
            next_id++;
        }

        private void Start()
        {
            Thread ctThread = new Thread(DoChat);
            ctThread.Start();
        }

        private void DoChat()
        {
            bool continuer = true;
            NetworkStream stream = this.clientSocket.GetStream();
            Messaging.SendMessage(stream, "Hi new user!");

            while (continuer)
            {
                
                try
                {
                    Byte[] bytes = new Byte[5];
                    int NombreOctets = stream.Read(bytes, 0, bytes.Length);
                    if(NombreOctets >= 5) //minimum number of bytes for CMD + length to follow
                    {
                        
                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        //Console.WriteLine($" >> command recieved from client {this.UserName} Id {this.Id} : {cmd} de taille {following_length} {NombreOctets}");

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
                catch (Exception ex) //à faire: prendre en compte la fermeture innatendue du canal par le client
                {
                    continuer = false;
                    Console.WriteLine(" >> " + ex.ToString());
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
    }
}
