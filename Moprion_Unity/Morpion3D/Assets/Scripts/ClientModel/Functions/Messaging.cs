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
        //Serialization

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
            client.StreamWrite(msg);
        }

        /// <summary>
        /// Handle the reception of a command <see cref="NomCommande.PNG"/> from the userhandhler which handles the <paramref name="client"/> on the server side
        /// </summary>
        /// <param name="client"></param>
        public static void RecievePing(byte[] bytes, Client client)
        {
            //client.LogWriter.Write("ping recieved from the server");
        }

        /// <summary>
        /// Handle the reception of a message
        /// </summary>
        /// <param name="client"></param>
        public static void RecieveMessage(byte[] bytes, Client client)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            client.LogWriter.Write("message recieved from the server: " + message);
        }

        /// <summary>
        /// Send the command to ask the server to send back the list of the others available users 
        /// </summary>
        /// <param name="client"></param>
        public static void AskOtherUsers(Client client)
        {
            byte[] msg = serializationMessage(NomCommande.OUS);
            client.StreamWrite(msg);
        }

        /// <summary>
        /// <para>Handle the reception of the list of the others available users</para>
        /// <para>Store the list of the others available users send by the server</para>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="client"></param>
        public static void RecieveOtherUsers(byte[] bytes, Client client)
        {
            int n_users = BitConverter.ToInt16(bytes, 0);

            Dictionary<int, User> newConnectedUsers = new Dictionary<int, User>();
            int byte_compt = 2;
            List<User> listUsers = new List<User>();
            for (int i = 0; i < n_users; i++)
            {
                int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
                string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
                User user = new User(user_id, userName);
                newConnectedUsers[user_id] = user;
                listUsers.Add(user);
            }
            client.RaiseOpponentListUpdated(listUsers);

            client.ConnectedUsers = new Dictionary<int, User>(newConnectedUsers);
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public static void SendMessage(Client client, string message)
        {
            byte[] msg = serializationMessage(message, NomCommande.MSG);
            client.StreamWrite(msg);
        }

        /// <summary>
        /// Send the new username to the server
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userName"></param>
        public static void SendUserName(Client client, string userName)
        {
            Debug.Log("Sending new username: " + userName);
            byte[] msg = serializationMessage(userName, NomCommande.USN);
            client.StreamWrite(msg);
        }

        // ---- Game Requests commands methods ----

        /// <summary>
        /// Send a Match Request to the client with the <paramref name="id"/> as id
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        public static void RequestMatch(Client client, int id)
        {
            byte[] msg = serializationMessage(BitConverter.GetBytes((Int16)id), NomCommande.MRQ);
            client.StreamWrite(msg);
        }

        /// <summary>
        /// <para>Handle the reception of the response to the <see cref="RequestMatch(Client, int)"/> send earlier</para>
        /// <para>If the response is positive the sender is store as the current opponent</para>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="client"></param>
        public static void RecieveGameRequestStatus(byte[] bytes, Client client)
        {
            Tuple<int, bool> tuple = deserializationResponseOpponent(bytes);
            int idOpponent = tuple.Item1;
            bool response = tuple.Item2;
            var user = client.ConnectedUsers[idOpponent];
            if (response)
            {
                client.Opponent = user;
            }
            var status = response ? MatchRequestEventArgs.EStatus.Accepted : MatchRequestEventArgs.EStatus.Declined;
            client.RaiseMatchRequestUpdated(new MatchRequestEventArgs(user, status));
        }

        /// <summary>
        /// Handle the reception of a new game request
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="client"></param>
        public static void RecieveGameRequest(byte[] bytes, Client client)
        {
            int byte_compt = 0;
            int user_id = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            int userName_length = BitConverter.ToInt16(bytes, byte_compt); byte_compt += 2;
            string userName = System.Text.Encoding.UTF8.GetString(bytes, byte_compt, userName_length); byte_compt += userName_length;
            User user = new User(user_id, userName);
            if (!(client.ConnectedUsers.ContainsKey(user_id)))
            {
                client.ConnectedUsers[user_id]= user;
            }
            client.gameRequestsRecieved[user_id] = user;
            client.RaiseMatchRequestUpdated(new MatchRequestEventArgs(user, MatchRequestEventArgs.EStatus.New));
        }

        /// <summary>
        /// Send the <paramref name="response"/> to the Game Request send by client with the id <paramref name="idOpponent"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="idOpponent"></param>
        /// <param name="response"></param>
        public static void SendGameRequestResponse(Client client, int idOpponent, bool response)
        {
            AskOtherUsers(client); 

            if (response)
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                client.StreamWrite(bytes);

                client.Opponent = client.ConnectedUsers[idOpponent];
                client.gameRequestsRecieved.Remove(idOpponent);
                var itemsToRemove = client.gameRequestsRecieved.ToArray();
                foreach (var opponent in itemsToRemove)
                {
                    bytes = serializationMessage(serializationResponseOpponent(opponent.Key, !response), NomCommande.GRR);
                    client.StreamWrite(bytes);
                    client.gameRequestsRecieved.Remove(opponent.Key);
                }
            }
            else
            {
                byte[] bytes = serializationMessage(serializationResponseOpponent(idOpponent, response), NomCommande.GRR);
                client.StreamWrite(bytes);
                client.gameRequestsRecieved.Remove(idOpponent);
            }
        }

        // ---- In-game commands methods ----

        /// <summary>
        /// Send to the server the <paramref name="position"/> that the client want to play
        /// </summary>
        /// <param name="client"></param>
        /// <param name="position"></param>
        public static void SendPositionPlayer(Client client, System.Numerics.Vector3 position)
        {
            byte[] positionBytes = Serialization.SerializationPositionPlayed(position);
            byte[] msg = serializationMessage(positionBytes, NomCommande.NPP);
            client.StreamWrite(msg);
        }

        /// <summary>
        /// Ask the server to send back the game board
        /// </summary>
        /// <param name="client"></param>
        public static void AskGameBoard(Client client)
        {
            byte[] msg = serializationMessage(NomCommande.DGB);
            client.StreamWrite(msg);
        }

        /// <summary>
        /// Handle the reception of a Game Board from the server
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="client"></param>
        public static void RecieveGameBoard(byte[] bytes, Client client)
        {
            client.GameClient = Serialization.DeserializationMatchStatus(bytes);
        }

        /// <summary>
        /// Handle the disconnection of the opponent of <paramref name="client"/>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="client"></param>
        public static void RecieveOpponentDisconnection(byte[] bytes, Client client)
        {
            Debug.Log("RaiseOpponentDisconnected");
            client.RaiseOpponentDisconnected();
        }
    }
}
