using Serveur.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serveur.Functions;
using System.Linq;

namespace Serveur
{
    
    /// <summary>
    /// a server that handles multiples clients, allowing games to take place
    /// </summary>
    public class Server
    {
        // ---- Static fields/properties ----

        private static int next_id = 0;

        // ---- Public fields/properties ----

        public readonly LogWriter LogWriter;

        private readonly object usersLock = new object();
        private Dictionary<int, UserHandler> userHandlers = new Dictionary<int, UserHandler>();
        public Dictionary<int, UserHandler> UsersHandlers
        {
            set
            {
                lock (usersLock)
                {
                    userHandlers = value;
                }
            }
            get
            {
                Dictionary<int, UserHandler> userHandlers_copy;
                lock (usersLock)
                {
                    userHandlers_copy = new Dictionary<int, UserHandler>(userHandlers);
                }
                return userHandlers_copy;

            }
        }



        // ---- Private fields/properties ----
        private readonly string LogFile = "serveur_log.txt";
        
        private int Port; 
        private IPAddress Ip;

        private bool keepRunning;

        private TcpListener tcpListener = null;
        private Thread listeningThread = null;

        // ---- Public methods ----
        
        public Server()
        {
            LogWriter = new LogWriter(LogFile);
            Port = 13000;
            Ip = IPAddress.Parse("127.0.0.1");
        }

        /// <summary>
        /// launch the server at <see cref="Ip"/> and <see cref="Port"/>
        /// </summary>
        public void Start()
        {
            keepRunning = true;

            tcpListener = new TcpListener(Ip, Port);
            tcpListener.Start();

            listeningThread = new Thread(ListenConnexion);
            listeningThread.Start();
        }

        /// <summary>
        /// stop the server
        /// </summary>
        public void Stop()
        {
            keepRunning = false;
            tcpListener.Stop();
        }

        // ---- Private methods ----

        /// <summary>
        /// wait for new clients to connect and create a <see cref="UserHandler"/> instance for each of them
        /// </summary>
        private void ListenConnexion()
        {
            while (keepRunning)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();

                    lock (usersLock)
                    {
                        userHandlers[next_id] = new UserHandler(client, next_id, this);
                        userHandlers[next_id].Start();
                    }

                    LogWriter.Write($"A new connexion has been made, the user has been asigned the id {next_id}");
                    next_id++;
                }
                catch (System.Net.Sockets.SocketException) // server has been shutdown
                {

                }
                

            }
            
            //shutdown every userHandler
            foreach (var userHandler in userHandlers.Values)
            {
                userHandler.KeepChatting = false;
            }
        }

        // ---- Main ----

        /// <summary>
        /// provide the console interface to monitor the server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            UserHandler.InnitMethods();

            Server myServer = new Server();
            Console.WriteLine("Bienvenue dans le gestionnaire serveur du Morpion3D");

            bool keepAsking = true;
            while (keepAsking)
            {
                Console.WriteLine("Que voulez-vous faire?" +
                    $"\n\t0-changer le port du serveur (current {myServer.Port})" +
                    $"\n\t1-changer l'adresse du serveur (current {myServer.Ip})" +
                    "\n\t2-lancer le serveur");
                string choice = Console.ReadLine();
                if (choice == "0")
                {

                    Console.WriteLine("entrez un port (ex 13000):");
                    string inputString = Console.ReadLine();

                    if (int.TryParse(inputString, out int new_port))
                    {
                        myServer.Port = new_port;
                    }
                    else
                    {
                        Console.WriteLine($"int.TryParse could not parse '{inputString}' to an int.");
                    }

                }
                else if (choice == "1")
                {
                    Console.WriteLine("entrez une adresse (ex 127.0.0.1)");
                    string inputString = Console.ReadLine();

                    if (IPAddress.TryParse(inputString, out IPAddress new_adress))
                    {
                        myServer.Ip = new_adress;
                    }
                    else
                    {
                        Console.WriteLine($"IPAddress.TryParse could not parse '{inputString}' to an IPAddress.");
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine($"\n\n>>  lancement du serveur sur le port {myServer.Port} de l'adresse {myServer.Ip}\n\n");
                    myServer.LogWriter.Write($"lancement du serveur sur le port {myServer.Port} de l'adresse {myServer.Ip}");
                    keepAsking = false;
                }
                else
                {
                    Console.WriteLine("Commande inconnue, réessayez");
                }
            }

            myServer.Start();

            keepAsking = true;
            while (keepAsking)
            {
                Console.WriteLine("Que voulez-vous faire?" +
                "\n\t0-afficher les utilisateurs connectés" +
                "\n\t1-afficher les matches en cours" +
                "\n\t2-éteindre le serveur");
                string choice = Console.ReadLine();
                if (choice == "0")
                {
                    var connectedUSers = from user in myServer.UsersHandlers.Values
                                          where user.Connected
                                          orderby user.Id, user.UserName
                                          select user;
                    Console.WriteLine($"Voici les {connectedUSers.Count()} utilisateurs connectés:");
                    foreach (var user in connectedUSers)
                    {
                        Console.WriteLine($"id {user.Id}, username: {user.UserName}");
                    }

                }
                else if (choice == "1")
                {
                    var ingameUsers = from user in myServer.UsersHandlers.Values
                                        where user.Connected && user.Game != null
                                        orderby user.Id, user.UserName
                                        select user;
                    Console.WriteLine($"Voici les {ingameUsers.Count()} utilisateurs en jeu:");
                    foreach (var user in ingameUsers)
                    {
                        Console.WriteLine($"id {user.Id}, username: {user.UserName}");
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine($"\n\n>>  extinction du serveur\n\n");
                    myServer.Stop();
                    myServer.LogWriter.Write($"arrêt du serveur");
                    keepAsking = false;
                }
                else
                {
                    Console.WriteLine("Commande inconnue, réessayez");
                }
            }
        }


    }
}
