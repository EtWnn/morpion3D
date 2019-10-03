using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serveur.Functions;

namespace Serveur.Models
{
    public class User
    {
        private static Mutex mutex = new Mutex();
        private static Dictionary<int, User> users = new Dictionary<int, User> { };
        public static Dictionary<int, User> Users
        {
            get
            {
                mutex.WaitOne();
                Dictionary<int, User> users_copy = new Dictionary<int, User>(users);
                mutex.ReleaseMutex();
                return users_copy;
                
            }
        }
        private static int next_id = 0;


        public int Id { get; set; }
        public string UserName { get; private set; }
        public TcpClient clientSocket { get; set; }

        public User(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            this.UserName = "default";
            this.Id = next_id;
            users[this.Id] = this; //on stocke le nouvel utilisateur dans le dictionnaire static users
            Console.WriteLine($" >> A new connexion has been made, the user has been asigned the id {this.Id}");
            users[this.Id].Start(); //on lance le canal de communication avec le nouvel utilisateur
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
            string bienvenue = $"You have been connected with the id {this.Id}, Please enter a username:";
            byte[] msg = Messaging.MessageToByte(bienvenue, 256);
            stream.Write(msg, 0, msg.Length);

            stream.Read(msg, 0, msg.Length);
            this.UserName = Messaging.ByteToMessage(msg);
            Console.WriteLine($" >> recieved from client Id {this.Id} the username {this.UserName}");

            bienvenue = $"Hello {this.UserName}! It is wonderfull to have you here";
            msg = Messaging.MessageToByte(bienvenue, 256);
            stream.Write(msg, 0, msg.Length);

            while (continuer)
            {
                
                try
                {
                    Byte[] bytes = new Byte[256];
                    int NombreOctets = stream.Read(bytes, 0, bytes.Length);
                    if(NombreOctets > 0)
                    {
                        string content = Messaging.ByteToMessage(bytes);
                        Console.WriteLine($" >> recieved from client {this.UserName} Id {this.Id} : {content}");
                    }
                    

                    
                }
                catch (Exception ex) //à faire: prendre en compte la fermeture innatendue du canal par le client
                {
                    continuer = false;
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }

        }
    }
}
