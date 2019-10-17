using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Client.ModelGame;

namespace Client.Functions
{
    public enum NomCommande
    {
        MSG,
        USN,
        OUS,
        MRQ,
        RQS,
        NPP,
        DGB,
        GRR

    }

    public class Messaging
    {
        //Serialization
        private static byte[] encodingMessage(string message, NomCommande nomCommande)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
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

            //renvoie le tableau de bytes
            return msg;
        }
        private static string serializationResponseOpponent(int idOpponent, bool response)
        {
            string message = idOpponent.ToString() + response.ToString();
            return message;
        }

        // General commands
        public static void RecieveMessage(byte[] bytes)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($" >> message recieved from the serveur: {message}");
        }

        public static void AskOtherUsers(NetworkStream stream)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(NomCommande.OUS.ToString());
            //length of the content in bytes
            var args_length = BitConverter.GetBytes((Int16)0); 
            


            byte[] msg = new byte[cmd.Length + args_length.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            args_length.CopyTo(msg, cmd.Length);

            //envoie de la requête
            stream.Write(msg, 0, msg.Length);

        }

        public static void RecieveOtherUsers(byte[] bytes, ref Dictionary<int, User> connected_users)
        {
            int n_users = BitConverter.ToInt16(bytes, 0);
            //Console.WriteLine($"I recieved {n_users} users");
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

        public static void SendUserName(NetworkStream stream, string userName)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(NomCommande.USN.ToString());
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

        // Game Requests commands

        public static void RequestMatch(NetworkStream stream, int id)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(NomCommande.MRQ.ToString());
            // id in bytes
            var id_bytes = BitConverter.GetBytes((Int16)id);
            //length of the id in bytes
            var args_length = BitConverter.GetBytes((Int16)id_bytes.Length);


            byte[] msg = new byte[cmd.Length + args_length.Length + id_bytes.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            args_length.CopyTo(msg, cmd.Length);
            //content
            id_bytes.CopyTo(msg, cmd.Length + args_length.Length);


            //envoie de la requête
            stream.Write(msg, 0, msg.Length);
        }

        public static void RecieveGameRequestStatus(byte[] bytes)
        {
            //lancer la partie ou retour au menu
        }

        public static void RecieveGameRequest(byte[] bytes, Client client)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;

            client.gameRequestsRecieved[user_id] = new User(user_id, userName);
        }

        // ADD new command for response to game request (with updating of the dictionary)
        public static void SendGameRequestResponse(NetworkStream stream, Client client, int idOpponent, bool response)
        {
            if (response)
            {
                byte[] bytes = encodingMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                stream.Write(bytes, 0, bytes.Length);
                client.Opponent = Client.connected_users[idOpponent];
                foreach (var opponent in client.gameRequestsRecieved)
                {
                    bytes = encodingMessage(serializationResponseOpponent(opponent.Key, !response), NomCommande.GRR);
                    stream.Write(bytes, 0, bytes.Length);
                    client.gameRequestsRecieved.Remove(opponent.Key);
                }
            }
            else
            {
                byte[] bytes = encodingMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                stream.Write(bytes, 0, bytes.Length);
                client.gameRequestsRecieved.Remove(idOpponent);
            }
        }

        // In-game commands
        public static void SendPositionPlayer(NetworkStream stream, Vector3 position)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(NomCommande.NPP.ToString());
            byte[] positionBytes = Serialization.SerializationPositionPlayed(position);
            //length of the content in bytes
            var args_length = BitConverter.GetBytes((Int16)positionBytes.Length);



            byte[] msg = new byte[cmd.Length + args_length.Length + positionBytes.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            args_length.CopyTo(msg, cmd.Length);
            //position
            positionBytes.CopyTo(msg, cmd.Length + args_length.Length);

            //envoie de la requête
            stream.Write(msg, 0, msg.Length);
        }

        public static void AskGameBoard(NetworkStream stream)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(NomCommande.DGB.ToString());
            //length of the content in bytes
            var args_length = BitConverter.GetBytes((Int16)0);



            byte[] msg = new byte[cmd.Length + args_length.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            args_length.CopyTo(msg, cmd.Length);

            //envoie de la requête
            stream.Write(msg, 0, msg.Length);

        }

        public static Game RecieveGameBoard(byte[] bytes)
        {
            return Serialization.DeserializationMatchStatus(bytes);
        }
    }
}
