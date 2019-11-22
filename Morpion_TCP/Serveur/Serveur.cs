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
    
    public class Serveur
    {
        private static int _next_id = 0;
        public const string log_file = "log.txt";


        public int Port = 13000; 
        public IPAddress Ip = IPAddress.Parse("127.0.0.1");

        private Mutex _usersMutex = new Mutex();
        private Dictionary<int, UserHandler> _userHandlers = new Dictionary<int, UserHandler>();
        public Dictionary<int, UserHandler> UsersHandlers
        {
            set
            {
                _usersMutex.WaitOne();
                _userHandlers = value;
                _usersMutex.ReleaseMutex();
            }
            get
            {
                _usersMutex.WaitOne();
                Dictionary<int, UserHandler> userHandlers_copy = new Dictionary<int, UserHandler>(_userHandlers);
                _usersMutex.ReleaseMutex();
                return userHandlers_copy;

            }
        }
        

        private bool _continuer;
        private const int CMD_SIZE = 4;

        private TcpListener _tcp_server = null;
        private Thread _listeningThread = null;

        public void Start()
        {

            Messaging.WriteLog(log_file, $"lancement du serveur sur le port {Port} de l'adresse {Ip}");

            _continuer = true;

            _tcp_server = new TcpListener(Ip, Port);
            _tcp_server.Start();

            _listeningThread = new Thread(() => ListenConnexion());
            _listeningThread.Start();
        }

        public void Stop()
        {
            _continuer = false;
        }

        private void ListenConnexion()
        {
            while (_continuer)
            {

                TcpClient client = _tcp_server.AcceptTcpClient();

                _usersMutex.WaitOne();
                _userHandlers[_next_id] = new UserHandler(client, _next_id, _userHandlers, _usersMutex, log_file);
                _userHandlers[_next_id].Start();
                _usersMutex.ReleaseMutex();

                //Console.WriteLine($" >> A new connexion has been made, the user has been asigned the id {_next_id}");
                Messaging.WriteLog(log_file, $"A new connexion has been made, the user has been asigned the id {_next_id}");
                _next_id++;

            }
        }

        static void Main(string[] args)
        {
            UserHandler.InnitMethods();

            Serveur my_serveur = new Serveur();
            Console.WriteLine("Bienvenue dans le gestionnaire serveur du Morpion3D");

            bool keep_asking = true;
            while (keep_asking)
            {
                Console.WriteLine("Que voulez-vous faire?" +
                    $"\n\t0-changer le port du serveur (current {my_serveur.Port})" +
                    $"\n\t1-changer l'adresse du serveur (current {my_serveur.Ip})" +
                    "\n\t2-lancer le serveur");
                string choice = Console.ReadLine();
                if (choice == "0")
                {

                    Console.WriteLine("entrez un port (ex 13000):");
                    string inputString = Console.ReadLine();

                    if (int.TryParse(inputString, out int new_port))
                    {
                        my_serveur.Port = new_port;
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
                        my_serveur.Ip = new_adress;
                    }
                    else
                    {
                        Console.WriteLine($"IPAddress.TryParse could not parse '{inputString}' to an IPAddress.");
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine($"\n\n>>  lancement du serveur sur le port {my_serveur.Port} de l'adresse {my_serveur.Ip}\n\n");
                    keep_asking = false;
                }
                else
                {
                    Console.WriteLine("Commande inconnue, réessayez");
                }
            }

            my_serveur.Start();

            keep_asking = true;
            while (keep_asking)
            {
                Console.WriteLine("Que voulez-vous faire?" +
                "\n\t0-afficher les utilisateurs connectés" +
                "\n\t1-afficher les matches en cours" +
                "\n\t2-éteindre le serveur");
                string choice = Console.ReadLine();
                if (choice == "0")
                {
                    var connected_users = from user in my_serveur.UsersHandlers.Values
                                      where user.IsAlive()
                                      orderby user.Id, user.UserName
                                      select user;

                    Console.WriteLine($"Voici les {connected_users.Count()} utilisateurs connectés:");
                    foreach (var user in connected_users)
                    {
                        Console.WriteLine($"id {user.Id}, username: {user.UserName}");
                    }

                }
                else if (choice == "1")
                {
                    var in_game_users = from user in my_serveur.UsersHandlers.Values
                                            where user.IsAlive() && user.Game != null
                                            orderby user.Id, user.UserName
                                            select user;

                    Console.WriteLine($"Voici les {in_game_users.Count()} utilisateurs en jeu:");
                    foreach (var user in in_game_users)
                    {
                        Console.WriteLine($"id {user.Id}, username: {user.UserName}");
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine($"\n\n>>  extinction du serveur\n\n");
                    my_serveur.Stop();
                    Messaging.WriteLog(log_file, $"arrêt du serveur");
                    keep_asking = false;
                }
                else
                {
                    Console.WriteLine("Commande inconnue, réessayez");
                }
            }
        }


    }
}
