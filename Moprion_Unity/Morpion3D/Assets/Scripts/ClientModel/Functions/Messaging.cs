using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MyClient.ModelGame;
using MyClient.Models;

namespace MyClient.Functions
{
    public enum NomCommande
    {
        MSG, //message
        USN, //username
        OUS, //other users
        MRQ, //match request
        NPP, //New position played 
        DGB, // game board
        GRR, // request response
        RGR // recieve game request
        //RQS, //

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

        private static Tuple<int, bool> deserializationResponseOpponent(byte[] bytes)
        {
            int byte_compt = 0;
            int idOpponent = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            bool response = BitConverter.ToBoolean(bytes, byte_compt);
            return Tuple.Create(idOpponent, response);
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
        
        public static void AskOtherUsers(NetworkStream stream)
        {
            byte[] msg = serializationMessage(NomCommande.OUS);
            stream.Write(msg, 0, msg.Length);
        }

        public static void RecieveOtherUsers(byte[] bytes, Client client)
        {
            int n_users = BitConverter.ToInt16(bytes, 0);
            client.connected_users = new Dictionary<int, User>();
            int byte_compt = 2;
            List<User> listUsers = new List<User>();
            for (int i = 0; i < n_users; i++)
            {
                int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
                User user = new User(user_id, userName);
                client.connected_users[user_id] = user;
                listUsers.Add(user);
            }
            client.RaiseOpponentListUpdated(listUsers);
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

        public static void RecieveGameRequestStatus(byte[] bytes, Client client)
        {
            Tuple<int, bool> tuple = deserializationResponseOpponent(bytes);
            int idOpponent = tuple.Item1;
            bool response = tuple.Item2;
            var user = client.connected_users[idOpponent];
            if (response)
            {
                client.Opponent = user;
            }
            var status = response ? MatchRequestEventArgs.EStatus.Accepted : MatchRequestEventArgs.EStatus.Canceled;
            client.RaiseMatchRequestUpdated(new MatchRequestEventArgs(user, status));
        }

        public static void RecieveGameRequest(byte[] bytes, Client client)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
            User user = new User(user_id, userName);
            if (!(client.connected_users.ContainsKey(user_id)))
            {
                client.connected_users[user_id]= user;
            }
            client.gameRequestsRecieved[user_id] = user;
            client.RaiseMatchRequestUpdated(new MatchRequestEventArgs(user, MatchRequestEventArgs.EStatus.New));
        }

        public static void SendGameRequestResponse(NetworkStream stream, Client client, int idOpponent, bool response)
        {
            AskOtherUsers(stream); //probablement a supprimer maintenant que le dictionnaire connected_users est mis a jour lors de l'appel de la methode RecieveGameRequest

            if (response)
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                stream.Write(bytes, 0, bytes.Length);

                client.Opponent = client.connected_users[idOpponent];
                client.gameRequestsRecieved.Remove(idOpponent);
                var itemsToRemove = client.gameRequestsRecieved.ToArray();
                foreach (var opponent in itemsToRemove)
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

        public static void RecieveGameBoard(byte[] bytes, Client client)
        {
            client.GameClient = Serialization.DeserializationMatchStatus(bytes);
        }
    }
}
