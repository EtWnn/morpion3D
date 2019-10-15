using Serveur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Serveur.Functions.Messaging
{
    public enum NomCommande
    {
        MSG,
        USN,
        OUS

    }

    public class Messaging
    {
        public static byte[] RecieveUserName(byte[] bytes, UserHandler userHandler)
        {
            string userName = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            userHandler.UserName = userName;
            Console.WriteLine($" >> message recieved from client Id {userHandler.Id} its new userName: {userHandler.UserName}");
            return new byte[0];
        }

        public static byte[] RecieveMessage(byte[] bytes, UserHandler userHandler)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($" >> message recieved from client {userHandler.UserName} Id {userHandler.Id} : {message}");
            return new byte[0];
        }

        public static byte[] SendOtherUsers(byte[] bytes, UserHandler userHandler)
        {
            
            var bytes_users = from e in UserHandler.UsersHandlers.Values
                        where e.Id != userHandler.Id
                        orderby e.Id, e.UserName
                        select e.ToBytes();
            int total_users_bytes = 0;
            foreach (var e in bytes_users)
            {
                total_users_bytes += e.Length;
            }

            //Console.WriteLine($"I have {bytes_users.Count()} other users connected");
            byte[] n_users_bytes = BitConverter.GetBytes((Int16)bytes_users.Count());

            byte[] cmd = Encoding.UTF8.GetBytes(NomCommande.OUS.ToString());
            byte[] length_bytes = BitConverter.GetBytes((Int16)(n_users_bytes.Length + total_users_bytes));
            

            byte[] response = new byte[cmd.Length + length_bytes.Length + n_users_bytes.Length + total_users_bytes];

            int compt = 0;
            cmd.CopyTo(response, compt); compt += cmd.Length;
            length_bytes.CopyTo(response, compt); compt += length_bytes.Length;
            n_users_bytes.CopyTo(response, compt); compt += n_users_bytes.Length;
            foreach(var e in bytes_users)
            {
                e.CopyTo(response, compt);
                compt += e.Length;
            }

            string cmd_string = System.Text.Encoding.UTF8.GetString(response, 0, response.Length);
            Console.WriteLine($" >> The client {userHandler.UserName} Id {userHandler.Id} asked for all connected user");
            //Console.WriteLine($" >> packet sent {cmd_string}");
            return response;
        }

        public static byte[] ReceiveNewPosition(byte[] bytes, UserHandler userHandler)
        {

        }
        public static void SendMessage(NetworkStream stream, string message)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(NomCommande.MSG.ToString());
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
        
    }
}
