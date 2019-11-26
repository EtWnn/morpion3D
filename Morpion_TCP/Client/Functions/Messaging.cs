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
        NPP, //New position played 
        DGB, // game board
        GRR, // request response
        RGR, // recieve game request
        NDC, // notification opponent disconnected
        PNG // Ping

    }

    public class Messaging
    {

        public static int StreamRead(MyClient client, byte[] message)
        {
            client.StreamMutex.WaitOne();
            int n_bytes = client.Stream.Read(message, 0, message.Length);
            client.StreamMutex.ReleaseMutex();

            return n_bytes;
        }

        public static void StreamWrite(MyClient client, byte[] message)
        {
            client.StreamMutex.WaitOne();
            client.Stream.Write(message, 0, message.Length);
            client.StreamMutex.ReleaseMutex();
        }

        //Serialization

        private static byte[] serializationMessage(NomCommande nomCommande)
        {
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            var message_length = BitConverter.GetBytes((Int16)0);

            byte[] msg = new byte[cmd.Length + message_length.Length];
            
            cmd.CopyTo(msg, 0);
            message_length.CopyTo(msg, cmd.Length);
            
            return msg;
        }
        private static byte[] serializationMessage(string message, NomCommande nomCommande)
        {
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            var message_bytes = Encoding.UTF8.GetBytes(message);
            var message_length = BitConverter.GetBytes((Int16)message_bytes.Length);
            

            byte[] msg = new byte[cmd.Length + message_length.Length + message_bytes.Length];
            
            cmd.CopyTo(msg, 0);
            message_length.CopyTo(msg, cmd.Length);
            message_bytes.CopyTo(msg, cmd.Length + message_length.Length);
            
            return msg;
        }
        private static byte[] serializationMessage(byte[] message_bytes, NomCommande nomCommande)
        {
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            var message_length = BitConverter.GetBytes((Int16)message_bytes.Length);

            byte[] msg = new byte[cmd.Length + message_length.Length + message_bytes.Length];
            
            cmd.CopyTo(msg, 0);
            message_length.CopyTo(msg, cmd.Length);
            message_bytes.CopyTo(msg, cmd.Length + message_length.Length);
            
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

        public static void SendPing(MyClient client)
        {
            byte[] msg = serializationMessage(NomCommande.PNG);
            StreamWrite(client, msg);
        }

        public static void RecieveMessage(byte[] bytes, MyClient client)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            WriteLog(client, "message recieved from the server: " + message);
        }

        public static void AskOtherUsers(MyClient client)
        {
            byte[] msg = serializationMessage(NomCommande.OUS);
            StreamWrite(client, msg);
        }

        public static void RecieveOtherUsers(byte[] bytes, MyClient client)
        {
            int n_users = BitConverter.ToInt16(bytes, 0);
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

        public static void SendMessage(MyClient client, string message)
        {
            byte[] msg = serializationMessage(message, NomCommande.MSG);
            StreamWrite(client, msg);
        }

        public static void SendUserName(MyClient client, string userName)
        {
            byte[] msg = serializationMessage(userName, NomCommande.USN);
            StreamWrite(client, msg);
        }

        // Game Requests commands

        public static void RequestMatch(MyClient client, int id)
        {
            byte[] msg = serializationMessage(BitConverter.GetBytes((Int16)id), NomCommande.MRQ);
            StreamWrite(client, msg);
        }

        public static void RecieveGameRequestStatus(byte[] bytes, MyClient client)
        {
            Tuple<int, bool> tuple = deserializationResponseOpponent(bytes);
            int idOpponent = tuple.Item1;
            bool response = tuple.Item2;
            if (response)
            {
                Console.WriteLine($">> l'identifiant de l'adversaire est {idOpponent}");
                Console.WriteLine($">> le dictionnaire client.connected_users est :");
                foreach (int key in client.connected_users.Keys)
                {
                    Console.WriteLine($"la clef est {key}");
                    client.connected_users[key].Display();
                }
                client.Opponent = client.connected_users[idOpponent];
            }
        }

        public static void RecieveGameRequest(byte[] bytes, MyClient client)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
            if (!(client.connected_users.ContainsKey(user_id)))
            {
                client.connected_users[user_id]= new User(user_id, userName);
            }
            client.gameRequestsRecieved[user_id] = new User(user_id, userName);
        }
        

        public static void SendGameRequestResponse(MyClient client, int idOpponent, bool response)
        {
            AskOtherUsers(client); 

            if (response)
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                StreamWrite(client, bytes);
                foreach (var key in client.connected_users.Keys)
                {
                    Console.WriteLine($"le dictionnaire client.connected_users contient l'id {key} en clef");
                    client.connected_users[key].Display();
                }
                client.Opponent = client.connected_users[idOpponent];
                client.gameRequestsRecieved.Remove(idOpponent);
                var itemsToRemove = client.gameRequestsRecieved.ToArray();
                foreach (var opponent in itemsToRemove)
                {
                    bytes = serializationMessage(serializationResponseOpponent(opponent.Key, !response), NomCommande.GRR);
                    StreamWrite(client, bytes);
                    client.gameRequestsRecieved.Remove(opponent.Key);
                }
            }
            else
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                StreamWrite(client, bytes);
                client.gameRequestsRecieved.Remove(idOpponent);
            }
        }

        // In-game commands
        public static void SendPositionPlayer(MyClient client, Vector3 position)
        {
            byte[] positionBytes = Serialization.SerializationPositionPlayed(position);
            byte[] msg = serializationMessage(positionBytes, NomCommande.NPP);
            StreamWrite(client, msg);
        }

        public static void AskGameBoard(MyClient client)
        {
            byte[] msg = serializationMessage(NomCommande.DGB);
            StreamWrite(client, msg);
        }

        public static void RecieveGameBoard(byte[] bytes, MyClient client)
        {
            Console.WriteLine($"GameBoard recue");
            client.GameClient = Serialization.DeserializationMatchStatus(bytes);
        }

        public static void WriteLog(MyClient client, string log)
        {
            DateTime localDate = DateTime.Now;
            string log_date = localDate.ToString("s");

            client.LogMutex.WaitOne();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(client.LogFile, true))
            {
                file.WriteLine(log_date + " " + log);
            }
            client.LogMutex.ReleaseMutex();
        }
    }

}
