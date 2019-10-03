using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serveur.Models;
using Client.Functions;

namespace Client2
{
    class Client
    {
        private static Int32 port = 13000;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");


        private Mutex mutex = new Mutex();
        private Dictionary<int, UserHandler> connected_users;

        static void Main(string[] args)
        {
            Console.WriteLine("Bonjour Client 2 !");

            IPEndPoint remoteEP = new IPEndPoint(localAddr, port);
            Socket sender = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine($"Press a key to Connect to the server {localAddr}");
            Console.ReadKey();
            Console.Write('\n');

            //connection to the server
            sender.Connect(remoteEP);
            NetworkStream stream = new NetworkStream(sender);

            //wait for the server to ask a username
            byte[] msg = new byte[256];
            int NombreOctets = stream.Read(msg, 0, msg.Length);
            Messaging.RecieveMessage(msg);

            //sending a user name
            string username = Console.ReadLine();
            Messaging.SendMessage(stream, username);

            //confirmation from the server
            msg = new byte[256];
            NombreOctets = stream.Read(msg, 0, msg.Length);
            Messaging.RecieveMessage(msg);


            //entering the commands loop
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine("Que voulez-vous faire?\n\t0-envoyer un message\n\t1-demander les utilisateurs connectés");
                string choice = Console.ReadLine();
                if (choice == "0")
                {

                    Console.WriteLine("entrez un message");
                    string message = Console.ReadLine();
                    Messaging.SendMessage(stream, message);

                }
                else if (choice == "1")
                {

                }
                else
                {
                    Console.WriteLine("Commande inconnue");
                }

            }

        }
    }
}
