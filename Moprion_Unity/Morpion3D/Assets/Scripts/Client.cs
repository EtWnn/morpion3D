using System;
using System.Collections.Generic;
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
    public class Client
    {
        public Int32 Port;
        public IPAddress ServerAdress;
        public event EventHandler Connected;
        public bool is_connected;

        private IPEndPoint remoteEP;
        private Socket sender;
        private NetworkStream stream;


        public Dictionary<int, User> connected_users = new Dictionary<int, User>();

        public Client(IPAddress serverAdress, Int32 port)
        {
            this.Port = port;
            this.ServerAdress = serverAdress;
            remoteEP = new IPEndPoint(this.ServerAdress, this.Port);
            sender = new Socket(this.ServerAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            is_connected = false;
        }

        public void Connect()
        {
            Thread ctThread = new Thread(AsyncConnect);
            ctThread.Start();
        }

        private void AsyncConnect()
        {
            sender.Connect(remoteEP);
            stream = new NetworkStream(sender);
            is_connected = true;
            Connected?.Invoke(this, EventArgs.Empty);
        }

        
    }
}
