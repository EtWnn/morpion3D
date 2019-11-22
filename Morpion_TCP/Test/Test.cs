using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serveur;
using MyClient;

namespace Test
{
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting tests");

            Console.WriteLine("Connexion test");
            ConnexionTest();



            Console.Read();

        }


        static void ConnexionTest()
        {
            Serveur.Serveur my_server = new Serveur.Serveur();
            MyClient.MyClient client1 = new MyClient.MyClient();
            MyClient.MyClient client2 = new MyClient.MyClient();

            my_server.Start();

            client1.tryConnect();
            client2.tryConnect();

            client1.tryDisconnect();
            client2.tryDisconnect();

            my_server.Stop();

        }
    }
}
