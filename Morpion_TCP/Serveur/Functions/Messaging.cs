using Serveur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serveur.Functions
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

            byte[] cmd = Encoding.UTF8.GetBytes("AID");
            byte[] length_bytes = BitConverter.GetBytes((Int16)total_users_bytes);

            byte[] response = new byte[cmd.Length + length_bytes.Length + total_users_bytes];

            cmd.CopyTo(response, 0);
            length_bytes.CopyTo(response, cmd.Length);

            int compt = cmd.Length + length_bytes.Length;
            foreach(var e in bytes_users)
            {
                e.CopyTo(response, compt);
                compt += e.Length;
            }


            Console.WriteLine($" >> The client {userHandler.UserName} Id {userHandler.Id} asked for all connected user");
            return response;
        }


        /* A message is composed by the commande "MSG" on 3 octets, the length of the content on 2 octets and the content
         * in short : MSG + length + content
         */
        public static byte[] MessageToByte(string message) 
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

            return msg;
        }

        public static string ByteToMessage(byte[] msg)
        {
            int len_content = BitConverter.ToInt16(msg, 3);
            string content = System.Text.Encoding.UTF8.GetString(msg, 5, len_content);
            return content;
        }


    }
}
