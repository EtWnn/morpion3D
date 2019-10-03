using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client.Functions;
using Serveur.Models;
using Client.Models;

namespace Client
{
    class Client
    {
        private static Int32 port = 13000;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        private static Mutex mutex = new Mutex();
        public static Dictionary<int, User> connected_users;
        static void DisplayOtherUser()
        {
            Console.WriteLine("Voici les autres utilisateurs:");
            foreach (var user in connected_users.Values)
            {
                user.Display();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Bonjour Client 1 !");

            IPEndPoint remoteEP = new IPEndPoint(localAddr, port);
            Socket sender = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine($"Press a key to Connect to the server {localAddr}");
            Console.ReadKey();
            Console.Write('\n');

            //connection to the server
            sender.Connect(remoteEP);
            NetworkStream stream = new NetworkStream(sender);

            //entering the commands loop
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine("Que voulez-vous faire?" +
                    "\n\t0-envoyer un message" +
                    "\n\t1-demander les utilisateurs connectés" +
                    "\n\t2-changer de UserName" +
                    "\n\t3-afficher les utilisateurs connectés");
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
                else if (choice == "2")
                {
                    Console.WriteLine("entrez un nom d'utilisateur");
                    string userName = Console.ReadLine();
                    Messaging.SendUserName(stream, userName);
                }
                else if (choice == "3")
                {
                    DisplayOtherUser();
                }
                else
                {
                    Console.WriteLine("Commande inconnue");
                }

            }

        }

        static void Listen(NetworkStream stream)
        {
            bool continuer = false;
            while (continuer)
            {

                try
                {
                    Byte[] bytes = new Byte[5];
                    int NombreOctets = stream.Read(bytes, 0, bytes.Length);
                    if (NombreOctets >= 5) //minimum number of bytes for CMD + length to follow
                    {

                        string cmd = System.Text.Encoding.UTF8.GetString(bytes, 0, 3);
                        int following_length = BitConverter.ToInt16(bytes, 3);

                        byte[] following_bytes = new byte[following_length];
                        stream.Read(following_bytes, 0, following_bytes.Length);

                        NomCommande cmd_type = (NomCommande)Enum.Parse(typeof(NomCommande), cmd);

                        if (cmd_type == NomCommande.MSG)
                        {
                            Messaging.RecieveMessage(following_bytes);
                        }
                        else if (cmd_type == NomCommande.USN)
                        {

                        }
                        else if (cmd_type == NomCommande.OUS)
                        {

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
    }
}
