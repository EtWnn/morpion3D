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

            int next_id = 0;
            Dictionary<int, UserHandler> userHandlers = new Dictionary<int, UserHandler>();
            Mutex usersMutex = new Mutex();
            UserHandler.InnitMethods();

            while (continuer)
            {

                TcpClient client = server.AcceptTcpClient();

                usersMutex.WaitOne();
                userHandlers[next_id] = new UserHandler(client, next_id, userHandlers, usersMutex);
                userHandlers[next_id].Start();
                usersMutex.ReleaseMutex();

                Console.WriteLine($" >> A new connexion has been made, the user has been asigned the id {next_id}");
                next_id++;

            }

        }
    }
}
