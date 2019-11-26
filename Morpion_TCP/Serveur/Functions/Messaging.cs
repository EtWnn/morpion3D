using Serveur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Serveur.ModelGame;
using System.Threading;

namespace Serveur.Functions
{
    /// <summary>
    /// Enumerate all the abbreviations of the commands the server may recieve
    /// </summary>
    public enum NomCommande
    {
        /// <summary>message</summary>
        MSG,
        /// <summary>username</summary>
        USN,
        /// <summary>other users</summary>
        OUS,
        /// <summary>new position played</summary>
        NPP,
        /// <summary> game board</summary>
        DGB,
        /// <summary>match request</summary>
        MRQ,
        /// <summary>game request response</summary>
        GRR,
        /// <summary>response game request</summary>
        RGR,
        /// <summary>notify opponent disconnection</summary>
        NDC,
        /// <summary>ping</summary>
        PNG
    }

    /// <summary>
    /// Group all the methods which write on or read the stream
    /// </summary>
    public class Messaging
    {
        /// ---- Serialization methods ----

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
        /// <para>Transform an id and a username in a list of <see cref="byte"/></para>
        /// </summary>
        /// <param name="idOpponent"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static byte[] serializationGameRequest(int id, string userName)
        {
            byte[] user_id_bytes = BitConverter.GetBytes((Int16)id);
            byte[] userName_bytes = Encoding.UTF8.GetBytes(userName);
            byte[] userName_lenght = BitConverter.GetBytes((Int16)userName_bytes.Length);

            byte[] message_bytes = new byte[user_id_bytes.Length + userName_lenght.Length + userName_bytes.Length];

            user_id_bytes.CopyTo(message_bytes, 0);
            userName_lenght.CopyTo(message_bytes, user_id_bytes.Length);
            userName_bytes.CopyTo(message_bytes, user_id_bytes.Length + userName_lenght.Length);

            return (message_bytes);

        }

        /// <summary>
        /// <para>Transform an id and the response to a match request in a list of <see cref="byte"/></para>
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

        /// ---- Deserialization commands methods ----

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


        /// ---- General commands methods ----

        /// <summary>
        /// convert a list <see cref="byte"/ into the new submitted username and update the <see cref="UserHandler"/>>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] RecieveUserName(byte[] bytes, UserHandler userHandler)
        {
            string UserName = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            userHandler.UserName = UserName;
            userHandler.ServerLogWriter.Write($"*** RecieveUserName: from user id {userHandler.Id}, UserName: {UserName}");
            return new byte[0];
        }

        /// <summary>
        /// convert a list of <see cref="byte"/> into the message sent by the server, writes it into the logs
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] RecieveMessage(byte[] bytes, UserHandler userHandler)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            userHandler.ServerLogWriter.Write($"*** RecieveMessage: from user id {userHandler.Id}, message: {message}");
            return new byte[0];
        }

        /// <summary>
        /// send a <see cref="string"/> message to the client
        /// </summary>
        /// <param name="userHandler"></param>
        /// <param name="message"></param>
        public static void SendMessage(UserHandler userHandler, string message)
        {

            var cmd = Encoding.UTF8.GetBytes(NomCommande.MSG.ToString());
            var message_length = BitConverter.GetBytes((Int16)message.Length);
            var message_bytes = Encoding.UTF8.GetBytes(message);


            byte[] msg = new byte[cmd.Length + message_length.Length + message_bytes.Length];


            cmd.CopyTo(msg, 0);
            message_length.CopyTo(msg, cmd.Length);
            message_bytes.CopyTo(msg, cmd.Length + message_length.Length);
            userHandler.StreamWrite(msg);

        }

        /// <summary>
        /// Handle the reception of a command <see cref="NomCommande.PNG"/> from the client
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] RecievePing(byte[] bytes, UserHandler userHandler)
        {
            ///userHandler.ServerLogWriter.Write($" >> ping recieved from user Id {userHandler.Id}"); // to much noise in the logs
            return new byte[0];
        }

        /// <summary>
        /// Send a ping to the client
        /// </summary>
        /// <param name="userHandler"></param>
        public static void SendPing(UserHandler userHandler)
        {
            byte[] msg = serializationMessage(new byte[0], NomCommande.PNG);
            userHandler.StreamWrite(msg);
        }

        /// <summary>
        /// create the <see cref="byte"/> list that encodes the list of the others available users 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] SendOtherUsers(byte[] bytes, UserHandler userHandler)
        {
            
            var bytesUsers = from e in userHandler.UsersHandlers.Values
                                    where e.Id != userHandler.Id && e.Connected
                                    orderby e.Id, e.UserName
                                    select e.ToBytes();
            int total_users_bytes = 0;
            foreach (var e in bytesUsers)
            {
                total_users_bytes += e.Length;
            }
            

            byte[] n_users_bytes = BitConverter.GetBytes((Int16)bytesUsers.Count());

            byte[] cmd = Encoding.UTF8.GetBytes(NomCommande.OUS.ToString());
            byte[] length_bytes = BitConverter.GetBytes((Int16)(n_users_bytes.Length + total_users_bytes));
            
            byte[] response = new byte[cmd.Length + length_bytes.Length + n_users_bytes.Length + total_users_bytes];

            int compt = 0;
            cmd.CopyTo(response, compt); compt += cmd.Length;
            length_bytes.CopyTo(response, compt); compt += length_bytes.Length;
            n_users_bytes.CopyTo(response, compt); compt += n_users_bytes.Length;
            foreach(var e in bytesUsers)
            {
                e.CopyTo(response, compt);
                compt += e.Length;
            }

            string cmd_string = System.Text.Encoding.UTF8.GetString(response, 0, response.Length);
            return response;
        }

        /// <summary>
        /// transfer a match request to the targeted player
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] TransferMatchRequest(byte[] bytes, UserHandler userHandler)
        {
            int idRecipient = BitConverter.ToInt16(bytes, 0);
            int idSender = userHandler.Id;

            userHandler.ServerLogWriter.Write($"*** TransferMatchRequest: try from {idSender} to {idRecipient}");
            string userNameSender = userHandler.UserName;

            byte[] msg = new byte[0];

            if (userHandler.UsersHandlers.ContainsKey(idRecipient) && userHandler.UsersHandlers[idRecipient].Game == null)
            {
                byte[] senderRequest_bytes = serializationGameRequest(idSender, userNameSender);
                byte[] request_msg = serializationMessage(senderRequest_bytes, NomCommande.MRQ);
                try
                {
                    userHandler.UsersHandlers[idRecipient].StreamWrite(request_msg);
                    userHandler.ServerLogWriter.Write($"*** TransferMatchRequest: success");
                }
                catch (Exception) // return a refusal if failed
                {
                    byte[] msg_bytes = serializationResponseOpponent(idRecipient, false);
                    msg = serializationMessage(msg_bytes, NomCommande.RGR);
                    userHandler.ServerLogWriter.Write($"*** TransferMatchRequest: failed");
                }



            }
            else // return a refusal
            {
                byte[] msg_bytes = serializationResponseOpponent(idRecipient, false);
                msg = serializationMessage(msg_bytes, NomCommande.RGR);

                if (!userHandler.UsersHandlers.ContainsKey(idRecipient))
                {
                    userHandler.ServerLogWriter.Write($"*** TransferMatchRequest: failed, the key {idRecipient} was not found");
                }
                else
                {
                    userHandler.ServerLogWriter.Write($"*** TransferMatchRequest: failed, the user id {idRecipient} is in a match");
                }

            }
            return msg;
        }

        /// <summary>
        /// transfer the response to a match request to client that emitted the request
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] TransferGameRequestResponse(byte[] bytes, UserHandler userHandler)
        {
            int idSender = userHandler.Id;
            Tuple<int, bool> tuple = deserializationResponseOpponent(bytes);
            int idRecipient = tuple.Item1;
            bool response = tuple.Item2;

            userHandler.ServerLogWriter.Write($"*** TransferGameRequestResponse: from {idSender} to {idRecipient}, accepted = {response}");

            byte[] msg_bytes = serializationResponseOpponent(idSender, response);
            byte[] msg_to_dest = serializationMessage(msg_bytes, NomCommande.RGR);
            userHandler.UsersHandlers[idRecipient].StreamWrite(msg_to_dest);

            msg_bytes = serializationResponseOpponent(idRecipient, response);
            byte[] msg_to_sender = serializationMessage(msg_bytes, NomCommande.RGR);
            userHandler.StreamWrite(msg_to_sender);

            if (response) //creation of the game object if the game request is accepted
            {
                Game game = new Game();
                game.SpecifyPlayersID(idSender, idRecipient);
                userHandler.Game = game;
                userHandler.UsersHandlers[idRecipient].Game = game;
                userHandler.UsersHandlers[idSender].Game = game;

                userHandler.ServerLogWriter.Write($"*** TransferGameRequestResponse: game object created");

                byte[] msg_board1 = SendGameBoard(new byte[0], userHandler.UsersHandlers[idRecipient]);
                userHandler.UsersHandlers[idRecipient].StreamWrite(msg_board1);

                byte[] msg_board2 = SendGameBoard(new byte[0], userHandler);
                userHandler.StreamWrite(msg_board2);
            }

            return new byte[0];
        }

        /// ---- InGame commands methods ----

        /// <summary>
        /// recieve the position played by the client, update and send the gameboard if the move is legal
        /// </summary>
        /// <param name="bytes">position played</param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] ReceivePositionPlayed(byte[] bytes, UserHandler userHandler)
        {
            userHandler.ServerLogWriter.Write($"*** ReceivePositionPlayed: from user id {userHandler.Id}");
            Vector3 position = Serialization.DeserializationPositionPlayed(bytes);
            if (userHandler.Game.Play(position, userHandler.Id))
            {
                int idPlayer1 = userHandler.Game.IdPlayer1;
                int idPlayer2 = userHandler.Game.IdPlayer2;
                userHandler.ServerLogWriter.Write($"*** ReceivePositionPlayed: success");

                byte[] msg_board1 = SendGameBoard(new byte[0], userHandler.UsersHandlers[userHandler.Game.IdPlayer1]);
                userHandler.UsersHandlers[idPlayer1].StreamWrite(msg_board1);

                byte[] msg_board2 = SendGameBoard(new byte[0], userHandler.UsersHandlers[userHandler.Game.IdPlayer2]);
                userHandler.UsersHandlers[idPlayer2].StreamWrite(msg_board2);

                if (!(userHandler.Game.Mode == GameMode.Player1 || userHandler.Game.Mode == GameMode.Player2))
                {
                    userHandler.UsersHandlers[idPlayer1].Game = null;
                    userHandler.UsersHandlers[idPlayer2].Game = null;
                }
            }
            else
            {
                userHandler.ServerLogWriter.Write($"*** ReceivePositionPlayed: failed, illegal move");
            }
            return new byte[0];
        }

        /// <summary>
        /// convert the <see cref="Game"/> into a list of <see cref="byte"/>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="userHandler"></param>
        /// <returns></returns>
        public static byte[] SendGameBoard(byte[] bytes, UserHandler userHandler)
        {
            byte[] bytesGame = Serialization.SerializationMatchStatus(userHandler.Game);
            byte[] response = serializationMessage(bytesGame, NomCommande.DGB);
            userHandler.ServerLogWriter.Write($"*** SendGameBoard: to user id {userHandler.Id}");
            return response;
        }

        /// <summary>
        /// send a notification to the opponent if the client was disconnected from the game
        /// </summary>
        /// <param name="userHandler"></param>
        public static void SendNotifcationDisconnection(UserHandler userHandler)
        {
            int idSender = userHandler.Id;
            int idRecipient = (userHandler.Id == userHandler.Game.IdPlayer1)? userHandler.Game.IdPlayer2 : userHandler.Game.IdPlayer1;
            byte[] msg_serialized=serializationMessage(new byte[0], NomCommande.NDC);
            userHandler.ServerLogWriter.Write( $"*** SendNotifcationDisconnection: try from {idSender} to {idRecipient}");
            userHandler.UsersHandlers[idRecipient].StreamWrite(msg_serialized);
            userHandler.ServerLogWriter.Write( $"*** SendNotifcationDisconnection: success");
        }
    }
}
