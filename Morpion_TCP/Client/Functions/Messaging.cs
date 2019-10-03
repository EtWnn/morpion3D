using Client.Models;
using Serveur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Functions
{
    public enum NomCommande
    {
        MSG,
        USN,
        OUS

    }

    public class Messaging
    {
        public static void RecieveMessage(byte[] bytes)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($" >> message recieved from the serveur: {message}");
        }

        public static void RecieveAllUsers(byte[] bytes, ref Dictionary<int, User> connected_users)
        {
            int n_users = BitConverter.ToInt16(bytes, 0);
            connected_users = new Dictionary<int, User>();
            int byte_compt = 2;
            for(int i = 0; i < n_users; i++)
            {
                int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;

                connected_users[user_id] = new User(user_id, userName);
            }

        }

        public static void SendMessage(NetworkStream stream, string message)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes("MSG");
            //length of the content in bytes
            var message_length = BitConverter.GetBytes((Int16)message.Length);
            //content in bytes
            var message_bytes = Encoding.UTF8.GetBytes(message);


            byte[] msg = new byte[cmd.Length + message_length.Length + message_bytes.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            message_length.CopyTo(msg, cmd.Length);
            //content
            message_bytes.CopyTo(msg, cmd.Length + message_length.Length);


            //envoie de la requête
            stream.Write(msg, 0, msg.Length);

        }

        public static void SendUserName(NetworkStream stream, string userName)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes("USN");
            //length of the content in bytes
            var message_length = BitConverter.GetBytes((Int16)userName.Length);
            //content in bytes
            var message_bytes = Encoding.UTF8.GetBytes(userName);


            byte[] msg = new byte[cmd.Length + message_length.Length + message_bytes.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            message_length.CopyTo(msg, cmd.Length);
            //content
            message_bytes.CopyTo(msg, cmd.Length + message_length.Length);


            //envoie de la requête
            stream.Write(msg, 0, msg.Length);

        }

    }
}
