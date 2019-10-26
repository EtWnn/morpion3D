using MyClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MyClient.ModelGame;

namespace MyClient.Functions
{
    public enum NomCommande
    {
        MSG, //message
        USN, //username
        OUS, //other users
        MRQ, //match request
        RQS,
        NPP, //New position played 
        DGB, // game board
        GRR, // request response
        RGR, // recieve game request

    }

    public class Messaging
    {
        //Serialization
        
        private static byte[] serializationMessage(NomCommande nomCommande)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            //length of the content in bytes
            var message_length = BitConverter.GetBytes((Int16)0);

            byte[] msg = new byte[cmd.Length + message_length.Length];

            //command
            cmd.CopyTo(msg, 0);
            //length to follow
            message_length.CopyTo(msg, cmd.Length);

            //renvoie le tableau de bytes
            return msg;
        }
        private static byte[] serializationMessage(string message, NomCommande nomCommande)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            //content in bytes
            var message_bytes = Encoding.UTF8.GetBytes(message);
            //length of the content in bytes
            var message_length = BitConverter.GetBytes((Int16)message_bytes.Length);
            

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
        private static byte[] serializationMessage(byte[] message_bytes, NomCommande nomCommande)
        {
            //command in bytes
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            //length of the content in bytes
            var message_length = BitConverter.GetBytes((Int16)message_bytes.Length);

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
        private static byte[] serializationResponseOpponent(int idOpponent, bool response)
        {
            byte[] idOpponent_bytes = BitConverter.GetBytes((Int16)idOpponent);
            byte[] response_bytes = BitConverter.GetBytes(response);
            byte[] message = new byte[idOpponent_bytes.Length + response_bytes.Length];
            idOpponent_bytes.CopyTo(message, 0);
            response_bytes.CopyTo(message, idOpponent_bytes.Length);
            return message;
        }

        
        private static User deserializationReceiveGameRequest(byte[] bytes)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
            return new User(user_id, userName);
        }

        // General commands
        public static void RecieveMessage(byte[] bytes)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($" >> message recieved from the serveur: {message}");
        }

        public static void AskOtherUsers(NetworkStream stream)
        {
            byte[] msg = serializationMessage(NomCommande.OUS);
            stream.Write(msg, 0, msg.Length);
        }

        public static void RecieveOtherUsers(byte[] bytes, MyClient client)
        {
            int n_users = BitConverter.ToInt16(bytes, 0);
            //Console.WriteLine($"I recieved {n_users} users");
            client.connected_users = new Dictionary<int, User>();
            int byte_compt = 2;
            for(int i = 0; i < n_users; i++)
            {
                int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;

                client.connected_users[user_id] = new User(user_id, userName);
            }

        }

        public static void SendMessage(NetworkStream stream, string message)
        {
            byte[] msg = serializationMessage(message, NomCommande.MSG);
            stream.Write(msg, 0, msg.Length);
        }

        public static void SendUserName(NetworkStream stream, string userName)
        {
            byte[] msg = serializationMessage(userName, NomCommande.USN);
            stream.Write(msg, 0, msg.Length);
        }

        // Game Requests commands

        public static void RequestMatch(NetworkStream stream, int id)
        {
            byte[] msg = serializationMessage(BitConverter.GetBytes((Int16)id), NomCommande.MRQ);
            stream.Write(msg, 0, msg.Length);
        }

        public static void RecieveGameRequestStatus(byte[] bytes)
        {
            //lancer la partie ou retour au menu
        }

        public static void RecieveGameRequest(byte[] bytes, MyClient client)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;

            client.gameRequestsRecieved[user_id] = new User(user_id, userName);
        }

        // ADD new command for response to game request (with updating of the dictionary)
        public static void SendGameRequestResponse(NetworkStream stream, MyClient client, int idOpponent, bool response)
        {
            AskOtherUsers(stream);
            if (response)
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                stream.Write(bytes, 0, bytes.Length);
                client.Opponent = client.connected_users[idOpponent];
                foreach (var opponent in client.gameRequestsRecieved)
                {
                    bytes = serializationMessage(serializationResponseOpponent(opponent.Key, !response), NomCommande.GRR);
                    stream.Write(bytes, 0, bytes.Length);
                    client.gameRequestsRecieved.Remove(opponent.Key);
                }
            }
            else
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                stream.Write(bytes, 0, bytes.Length);
                client.gameRequestsRecieved.Remove(idOpponent);
            }
        }

        // In-game commands
        public static void SendPositionPlayer(NetworkStream stream, Vector3 position)
        {
            byte[] positionBytes = Serialization.SerializationPositionPlayed(position);
            byte[] msg = serializationMessage(positionBytes, NomCommande.NPP);
            stream.Write(msg, 0, msg.Length);
        }

        public static void AskGameBoard(NetworkStream stream)
        {
            byte[] msg = serializationMessage(NomCommande.DGB);
            stream.Write(msg, 0, msg.Length);
        }

        public static Game RecieveGameBoard(byte[] bytes)
        {
            return Serialization.DeserializationMatchStatus(bytes);
        }
    }
}
