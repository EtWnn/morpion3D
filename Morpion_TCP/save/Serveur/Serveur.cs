using Serveur.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Serveur
{
    class Serveur
    {
        private static Int32 port = 13000;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        private static bool continuer = true;
        private const int CMD_SIZE = 4;


        static void Main(string[] args)
        {
            TcpListener server = null;
            server = new TcpListener(localAddr, port);
            server.Start();

            while (continuer)
            {

                TcpClient client = server.AcceptTcpClient();
                User new_user = new User(client);

            }

        }
    }
}
