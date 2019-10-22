using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyClient.Functions;
using MyClient.Models;

namespace MyClient
{
    public class MyClient
    {
        public Int32 port = 13000;
        public IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        private Socket _socket = null;
        private IPEndPoint _remoteEP = null;
        private bool _continueListen = false;
        private Thread _listeningThread = null;
        public NetworkStream Stream = null;


        public Mutex mutex = new Mutex();
        public Dictionary<int, User> connected_users = new Dictionary<int, User>();


        public Dictionary<int, User> gameRequestsRecieved = new Dictionary<int, User>();
        public User Opponent = null;
        
        public void tryConnect()
        {
            if( this._socket == null || !this._socket.Connected)
            {
                this._remoteEP = new IPEndPoint(this.localAddr, this.port);
                this._socket = new Socket(this.localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //connection to the server
                this._socket.Connect(this._remoteEP);
                if (this._socket.Connected)
                {
                    this.Stream = new NetworkStream(this._socket);
                    this.Stream.ReadTimeout = 10;

                    //launching the listening thread
                    this._listeningThread = new Thread(() => this.Listen(this.Stream));
                    this._listeningThread.Start();
                }
            }
            
            
        }

        public void tryDisconnect()
        {
            if (this._socket != null && this._socket.Connected)
            {
                this._continueListen = false;
                this._socket.Dispose();
                Console.WriteLine("Déconnection effectuée");
            }
        }

        void Listen(NetworkStream stream)
        {
            this._continueListen = true;
            while (this._continueListen)
            {

                try
                {
                    Byte[] bytes = new Byte[5];
                    int NombreOctets = 0;
                    try
                    {
                        NombreOctets = stream.Read(bytes, 0, bytes.Length);
                    }
                    catch (IOException ex)
                    {
                        NombreOctets = 0;
                    }
                    
                    if (NombreOctets >= 5) //minimum number of bytes for CMD + length to follow
                    {

                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        byte[] following_bytes = new byte[following_length];
                        stream.Read(following_bytes, 0, following_bytes.Length);

                        //Console.WriteLine($" >> command recieved from the serveur : {cmd} de taille {following_length} {NombreOctets}");

                        string packet_string = System.Text.Encoding.UTF8.GetString(following_bytes, 0, following_bytes.Length);
                        NomCommande cmd_type = (NomCommande)Enum.Parse(typeof(NomCommande), cmd);

                        if (cmd_type == NomCommande.MSG)
                        {
                            Messaging.RecieveMessage(following_bytes);
                        }
                        else if (cmd_type == NomCommande.OUS)
                        {
                            Messaging.RecieveOtherUsers(following_bytes, this);
                        }
                        else if (cmd_type == NomCommande.RGR)
                        {
                            Messaging.RecieveGameRequest(following_bytes, this);
                        }
                        else if (cmd_type == NomCommande.RGR)
                        {
                            Messaging.RecieveGameRequest(following_bytes, this);
                        }

                    }



                }
                catch (Exception ex) //à faire: prendre en compte la fermeture innatendue du canal par le serveur
                {
                    this._continueListen = false;
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
        }

        void DisplayOtherUser()
        {
            Console.WriteLine($"Voici les {connected_users.Count} autres utilisateurs:");
            foreach (var user in connected_users.Values)
            {
                user.Display();
            }
        }

        void DisplayMatchRequest()
        {
            Console.WriteLine($"Voici les {gameRequestsRecieved.Count} requêtes reçues");
            foreach (var r in gameRequestsRecieved.Values)
            {
                r.Display();
            }
        }

        static void Main(string[] args)
        {
            MyClient my_client = new MyClient();
            Console.WriteLine("Bonjour Client !");

            //entering the commands loop
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine("Que voulez-vous faire?" +
                    "\n\t0-envoyer un message" +
                    "\n\t1-demander les utilisateurs connectés"+
                    "\n\t2-changer de UserName" +
                    "\n\t3-afficher les utilisateurs connectés" +
                    "\n\t4-afficher les rêquetes de match" +
                    "\n\t5-répondre à une requête de match" +
                    "\n\t6-exprimer une requête de match" +
                    "\n\t7-se déconnecter" +
                    "\n\t8-se connecter");
                string choice = Console.ReadLine();
                if (choice == "0")
                {

                    Console.WriteLine("entrez un message");
                    string message = Console.ReadLine();
                    Messaging.SendMessage(my_client.Stream, message);

                }
                else if (choice == "1")
                {
                    Messaging.AskOtherUsers(my_client.Stream);
                    Console.WriteLine("La demande a été émise");
                }
                else if (choice == "2")
                {
                    Console.WriteLine("entrez un nom d'utilisateur");
                    string userName = Console.ReadLine();
                    Messaging.SendUserName(my_client.Stream, userName);
                }
                else if (choice == "3")
                {
                    my_client.DisplayOtherUser();
                }
                else if (choice == "4")
                {
                    my_client.DisplayMatchRequest();
                }
                else if(choice == "5")
                {
                    Console.WriteLine("entrez l'id de l'adversaire:");
                    int id = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("entrez votre réponse:");
                    bool accepted = Convert.ToBoolean(Console.ReadLine());
                    Messaging.SendGameRequestResponse(my_client.Stream, my_client, id, accepted);
                }
                else if(choice == "6")
                {
                    Console.WriteLine("Entrez l'id de l'adversaire souhaité:");
                    int id = Convert.ToInt32(Console.ReadLine());
                    Messaging.RequestMatch(my_client.Stream, id);
                    Console.WriteLine("Requête envoyée");
                }
                else if (choice == "7")
                {
                    my_client.tryDisconnect();
                    
                }
                else if (choice == "8")
                {
                    my_client.tryConnect();
                    Console.WriteLine($"Connected to the server {my_client.localAddr}");
                }
                else
                {
                    Console.WriteLine("Commande inconnue");
                }

            }

        }

        
    }
}
