using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Client2.Functions;

namespace Client2
{
    class Client2
    {
        private static Int32 port = 13000;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");


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
            Console.Write(Messaging.ByteToMessage(msg));

            //sending a user name
            string username = Console.ReadLine();
            msg = Messaging.MessageToByte(username, 256);
            stream.Write(msg, 0, msg.Length);

            //confirmation from the server
            msg = new byte[256];
            NombreOctets = stream.Read(msg, 0, msg.Length);
            Console.WriteLine(Messaging.ByteToMessage(msg));


            //entering the commands loop
            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine("entrez un message");
                string message = Console.ReadLine();

                msg = Messaging.MessageToByte(message, 256);
                stream.Write(msg, 0, msg.Length);
            }

        }
    }
}
