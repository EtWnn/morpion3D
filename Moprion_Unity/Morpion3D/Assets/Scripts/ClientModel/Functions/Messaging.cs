using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MyClient.ModelGame;
using MyClient.Models;
using UnityEngine;

namespace MyClient.Functions
{
    /// <summary>
    /// Enumerate all the abbreviations of the commands the client may recieve
    /// </summary>
    public enum NomCommande
    {
        /// <summary>message</summary>
        MSG,
        /// <summary>username</summary>
        USN, 
        /// <summary>other users</summary>
        OUS, 
        /// <summary>match request</summary>
        MRQ, 
        /// <summary>new position played</summary>
        NPP,
        /// <summary>game board</summary>
        DGB,
        /// <summary>request response</summary>
        GRR,
        /// <summary>recieve game request</summary>
        RGR,
        /// <summary>notification opponent disconnected</summary>
        NDC,
        /// <summary>ping</summary>
        PNG 

    }

    /// <summary>
    /// Group all the methods which write on or read the stream
    /// </summary>
    public class Messaging
    {
        // ---- Public Static methods ----

        // ---- Streaming methods ----

        /// <summary>
        /// <para>Read safely a number of <see cref="byte"/> equal to the lenght of <paramref name="message"/> on the <see cref="Client.Stream"/> of the <paramref name="client"/></para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public static int StreamRead(Client client, byte[] message)
        {
            client.StreamMutex.WaitOne();
            int n_bytes = client.Stream.Read(message, 0, message.Length);
            client.StreamMutex.ReleaseMutex();

            return n_bytes;
        }

        /// <summary>
        /// Write safely the <paramref name="message"/> on the <see cref="Client.Stream"/> of the <paramref name="client"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public static void StreamWrite(Client client, byte[] message)
        {
            client.StreamMutex.WaitOne();
            client.Stream.Write(message, 0, message.Length);
            client.StreamMutex.ReleaseMutex();
        }

        // ---- Serialization methods ----

        /// <summary>
        /// Transform a <see cref="NomCommande"/> in a list of <see cref="byte"/>
        /// <para>Can be deserialized by the methods the server</para>
        /// </summary>
        /// <param name="nomCommande"></param>
        /// <returns></returns>
        private static byte[] serializationMessage(NomCommande nomCommande)
        {
            
            var cmd = Encoding.UTF8.GetBytes(nomCommande.ToString());
            var message_length = BitConverter.GetBytes((Int16)0);

            byte[] msg = new byte[cmd.Length + message_length.Length];
            
            cmd.CopyTo(msg, 0);
            message_length.CopyTo(msg, cmd.Length);
            
            return msg;
        }

        /// <summary>
        /// <para>Transform a <see cref="NomCommande"/> and a <see cref="string"/> in a list of <see cref="byte"/></para>
        /// <para>Can be deserialized by the methods the server</para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="nomCommande"></param>
        /// <returns></returns>
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

        /// <summary>
        /// <para>Transform a <see cref="NomCommande"/> and a list of <see cref="byte"/> in a list of <see cref="byte"/></para>
        /// <para>Can be deserialized by the methods the server</para>
        /// </summary>
        /// <param name="message_bytes"></param>
        /// <param name="nomCommande"></param>
        /// <returns></returns>
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

        /// <summary>
        /// <para>Transform a <see cref="int"/> representing an id and a list of <see cref="bool"/> in a list of <see cref="byte"/></para>
        /// <para>Can be deserialized by <see cref="deserializationResponseOpponent(byte[])"/></para>
        /// </summary>
        /// <param name="idOpponent"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static byte[] serializationResponseOpponent(int idOpponent, bool response)
        {
            byte[] idOpponent_bytes = BitConverter.GetBytes((Int16)idOpponent);
            byte[] response_bytes = BitConverter.GetBytes(response);
            byte[] message = new byte[idOpponent_bytes.Length + response_bytes.Length];
            idOpponent_bytes.CopyTo(message, 0);
            response_bytes.CopyTo(message, idOpponent_bytes.Length);
            return message;
        }

        /// <summary>
        /// Deserialized a list of <see cref="byte"/> serialized by the method <see cref="serializationResponseOpponent(int, bool)"/>
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static Tuple<int, bool> deserializationResponseOpponent(byte[] bytes)
        {
            int byte_compt = 0;
            int idOpponent = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            bool response = BitConverter.ToBoolean(bytes, byte_compt);
            return Tuple.Create(idOpponent, response);
        }

        /// <summary>
        /// Return an object <see cref="User"/> corresponding to the client who send the game request
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static User deserializationReceiveGameRequest(byte[] bytes)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
            return new User(user_id, userName);
        }

        // ---- General commands methods ----

        /// <summary>
        /// Send a command <see cref="NomCommande.PNG"/> to the userhandhler which handles the <paramref name="client"/> on the server side
        /// </summary>
        /// <param name="client"></param>
        public static void SendPing(Client client)
        {
            byte[] msg = serializationMessage(NomCommande.PNG);
            StreamWrite(client, msg);
        }

        /// <summary>
        /// Recieve a command <see cref="NomCommande.PNG"/> from the userhandhler which handles the <paramref name="client"/> on the server side
        /// </summary>
        /// <param name="client"></param>
        public static void RecievePing(byte[] bytes, Client client)
        {
            WriteLog(client, "ping recieved from the server:");
        }

        /// <summary>
        /// Recieve a message
        /// </summary>
        /// <param name="client"></param>
        public static void RecieveMessage(byte[] bytes, Client client)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            WriteLog(client, "message recieved from the server: " + message);
        }

        /// <summary>
        /// Send the command to ask the server to send back the list of the others available users 
        /// </summary>
        /// <param name="client"></param>
        public static void AskOtherUsers(Client client)
        {
            byte[] msg = serializationMessage(NomCommande.OUS);
            StreamWrite(client, msg);
        }

        /// <summary>
        /// Store the list of the others available users send by the server
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="client"></param>
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

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public static void SendMessage(Client client, string message)
        {
            byte[] msg = serializationMessage(message, NomCommande.MSG);
            StreamWrite(client, msg);
        }

        /// <summary>
        /// Send the new username to the server
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userName"></param>
        public static void SendUserName(Client client, string userName)
        {
            byte[] msg = serializationMessage(userName, NomCommande.USN);
            StreamWrite(client, msg);
        }

        // ---- Game Requests commands methods ----

        public static void RequestMatch(Client client, int id)
        {
            byte[] msg = serializationMessage(BitConverter.GetBytes((Int16)id), NomCommande.MRQ);
            StreamWrite(client, msg);
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
            var status = response ? MatchRequestEventArgs.EStatus.Accepted : MatchRequestEventArgs.EStatus.Declined;
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

        public static void SendGameRequestResponse(Client client, int idOpponent, bool response)
        {
            AskOtherUsers(client); //probablement a supprimer maintenant que le dictionnaire connected_users est mis a jour lors de l'appel de la methode RecieveGameRequest

            if (response)
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                StreamWrite(client, bytes);

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

        // ---- In-game commands methods ----

        public static void SendPositionPlayer(Client client, System.Numerics.Vector3 position)
        {
            byte[] positionBytes = Serialization.SerializationPositionPlayed(position);
            byte[] msg = serializationMessage(positionBytes, NomCommande.NPP);
            StreamWrite(client, msg);
        }

        public static void AskGameBoard(Client client)
        {
            byte[] msg = serializationMessage(NomCommande.DGB);
            StreamWrite(client, msg);
        }

        public static void RecieveGameBoard(byte[] bytes, Client client)
        {
            client.GameClient = Serialization.DeserializationMatchStatus(bytes);
        }

        public static void RecieveOpponentDisconnection(byte[] bytes, Client client)
        {
            Debug.Log("RaiseOpponentDisconnected");
            client.RaiseOpponentDisconnected();
        }

        // ---- Log methods ----

        public static void WriteLog(Client client, string log)
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
