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
using MyClient.ModelGame;
using System.Numerics;

namespace MyClient
{
    public class Client
    {
        public Int32 port = 13000;
        public IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        private Socket _socket = null;
        private IPEndPoint _remoteEP = null;
        private bool _continueListen = false;
        private Thread _listeningThread = null;
        public NetworkStream Stream = null;

        public event EventHandler Connected;
        public bool is_connected = false;


        public Mutex mutex = new Mutex();
        public Dictionary<int, User> connected_users = new Dictionary<int, User>();


        public Dictionary<int, User> gameRequestsRecieved = new Dictionary<int, User>();
        public User Opponent = null;
        public Game GameClient = null;

        private static Dictionary<NomCommande, Action<byte[], Client>> methods = new Dictionary<NomCommande, Action<byte[], Client>>();
        public static void InnitMethods()
        {
            methods[NomCommande.OUS] = Messaging.RecieveOtherUsers;
            methods[NomCommande.RGR] = Messaging.RecieveGameRequestStatus;
            methods[NomCommande.MRQ] = Messaging.RecieveGameRequest;
            methods[NomCommande.DGB] = Messaging.RecieveGameBoard;
        }

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

                    is_connected = true;
                    Connected?.Invoke(this, EventArgs.Empty);
                }
            }
            
            
        }

        public void tryDisconnect()
        {
            if (this._socket != null && this._socket.Connected)
            {
                this._continueListen = false;
                this._socket.Disconnect(false);

                is_connected = false;
                Connected?.Invoke(this, EventArgs.Empty);
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
                        if (following_length > 0)
                        {
                            stream.Read(following_bytes, 0, following_bytes.Length);
                        }
                        
                        string packet_string = System.Text.Encoding.UTF8.GetString(following_bytes, 0, following_bytes.Length);
                        NomCommande cmd_type = (NomCommande)Enum.Parse(typeof(NomCommande), cmd);

                        if (cmd_type == NomCommande.MSG)
                        {
                            Messaging.RecieveMessage(following_bytes);
                        }
                        else
                        {
                            Client.methods[cmd_type](following_bytes, this);
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

        void DisplayGameBoard()
        {
            Console.WriteLine($">>Voici le plateau du jeu");
            GameBoard.display(this.GameClient.GameBoardMatrix); //ajouter une exception
            if ((this.GameClient.Mode == GameMode.Player1 && this.GameClient.IdPlayer1 != this.Opponent.Id)|| (this.GameClient.Mode == GameMode.Player2 && this.GameClient.IdPlayer2 != this.Opponent.Id))
            {
                Console.WriteLine(">> C'est a votre tour de jouer");
            }
            else if ((this.GameClient.Mode == GameMode.Player1Won && this.GameClient.IdPlayer1 != this.Opponent.Id) || (this.GameClient.Mode == GameMode.Player2Won && this.GameClient.IdPlayer2 != this.Opponent.Id))
            {
                Console.WriteLine(">> Vous avez gagné");
            }
            else if ((this.GameClient.Mode == GameMode.Player1Won) || (this.GameClient.Mode == GameMode.Player2Won ))
            {
                Console.WriteLine(">> Vous avez perdu");
            }
            else
            {
                Console.WriteLine(">> Ce n'est pas a votre tour de jouer");
            }
        }

        static void Main(string[] args)
        {
            Client my_client = new Client();
            Client.InnitMethods();
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
                    "\n\t8-se connecter" +
                    "\n\t9-afficher l'id de l'adversaire" +
                    "\n\t10-actualiser le plateau" +
                    "\n\t11-afficher le plateau" +
                    "\n\t12-Jouer une position");
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
                else if (choice == "9")
                {
                    if (my_client.Opponent != null)
                    {
                        Console.WriteLine($"L'id de votre adversaire est: {my_client.Opponent.Id} et son user name est: {my_client.Opponent.UserName}");
                    }
                    else
                    {
                        Console.WriteLine($"Aucun adversaire n'est attribué");
                    }
                }
                else if (choice == "10")
                {
                    Messaging.AskGameBoard(my_client.Stream);
                    Console.WriteLine($"Requête envoyée");
                }
                else if (choice == "11")
                {
                    my_client.DisplayGameBoard();
                }
                else if (choice == "12")
                {
                    Console.WriteLine($"my_client.GameClient.Mode : {my_client.GameClient.Mode }; GameMode.Player1: {GameMode.Player1}; GameMode.Player2 : {GameMode.Player2}");
                    Console.WriteLine($"my_client.GameClient.IdPlayer1 : {my_client.GameClient.IdPlayer1 }; my_client.GameClient.IdPlayer2: {my_client.GameClient.IdPlayer2}; my_client.Opponent.Id : {my_client.Opponent.Id}");
                    {
                        Vector3 position = new Vector3();
                        int x = 0;
                        int y = 0;
                        int z = 0;
                        Console.WriteLine("Quelle est la coordonnee x (0,1 ou 2) de la position que vous voulez jouer ? (La couche)");
                        x = (int.Parse(Console.ReadLine()));
                        position.X = x;
                        Console.WriteLine("Quelle est la coordonnee y (0,1 ou 2) de la position que vous voulez jouer ? (la ligne) ");
                        y = (int.Parse(Console.ReadLine()));
                        position.Y = y;
                        Console.WriteLine("Quelle est la coordonnee z (0,1 ou 2) de la position que vous voulez jouer ? (la colonne)");
                        z = (int.Parse(Console.ReadLine()));
                        position.Z = z;
                        Messaging.SendPositionPlayer(my_client.Stream, position);
                    }
                }
                else
                {
                    Console.WriteLine("Commande inconnue");
                }

            }

        }

        
    }
}
