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
    public enum NomCommande
    {
        MSG,
        USN,
        OUS,
        NPP,
        DGB,
        MRQ,
        GRR,
        RGR,
        NDC,
        PNG
    }

    public class Messaging
    {
        public static int StreamRead(UserHandler userHandler, byte[] message)
        {
            userHandler.StreamMutex.WaitOne();
            int n_bytes = userHandler.Stream.Read(message, 0, message.Length);
            userHandler.StreamMutex.ReleaseMutex();

            return n_bytes;
        }

        public static void StreamWrite(UserHandler userHandler, byte[] message)
        {
            userHandler.StreamMutex.WaitOne();
            userHandler.Stream.Write(message, 0, message.Length);
            userHandler.StreamMutex.ReleaseMutex();
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

        public static byte[] RecieveUserName(byte[] bytes, UserHandler userHandler)
        {
            string UserName = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            userHandler.UserName = UserName;
            Messaging.WriteLog(userHandler, $"*** RecieveUserName: from user id {userHandler.Id}, UserName: {UserName}");
            return new byte[0];
        }

        public static byte[] RecieveMessage(byte[] bytes, UserHandler userHandler)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Messaging.WriteLog(userHandler, $"*** RecieveMessage: from user id {userHandler.Id}, message: {message}");
            return new byte[0];
        }

        public static byte[] RecievePing(byte[] bytes, UserHandler userHandler)
        {
            Messaging.WriteLog(userHandler, $"*** RecievePing: from user id {userHandler.Id}");
            return new byte[0];
        }

        public static byte[] SendOtherUsers(byte[] bytes, UserHandler userHandler)
        {
            
            var bytes_users = from e in userHandler.UsersHandlers.Values
                        where e.Id != userHandler.Id && e.ClientSocket.Connected
                              orderby e.Id, e.UserName
                        select e.ToBytes();
            int total_users_bytes = 0;
            foreach (var e in bytes_users)
            {
                total_users_bytes += e.Length;
            }
            

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
            return response;
        }

        public static byte[] ReceivePositionPlayed(byte[] bytes, UserHandler userHandler)
        {
            Messaging.WriteLog(userHandler, $"*** ReceivePositionPlayed: from user id {userHandler.Id}");
            Vector3 position = Serialization.DeserializationPositionPlayed(bytes);
            if (userHandler.Game.Play(position, userHandler.Id))
            {
                int idPlayer1 = userHandler.Game.IdPlayer1;
                int idPlayer2 = userHandler.Game.IdPlayer2;
                Messaging.WriteLog(userHandler, $"*** ReceivePositionPlayed: success");

                byte[] msg_board1 = SendGameBoard(new byte[0], userHandler.UsersHandlers[userHandler.Game.IdPlayer1]);
                StreamWrite(userHandler.UsersHandlers[idPlayer1], msg_board1);

                byte[] msg_board2 = SendGameBoard(new byte[0], userHandler.UsersHandlers[userHandler.Game.IdPlayer2]);
                StreamWrite(userHandler.UsersHandlers[idPlayer2], msg_board2);

                if (!(userHandler.Game.Mode == GameMode.Player1 || userHandler.Game.Mode == GameMode.Player2))
                {
                    userHandler.UsersHandlers[idPlayer1].Game = null;
                    userHandler.UsersHandlers[idPlayer2].Game = null;
                }
            }
            else
            {
                Messaging.WriteLog(userHandler, $"*** ReceivePositionPlayed: failed, illegal move");
            }
            return new byte[0];
        }

        public static byte[] SendGameBoard(byte[] bytes, UserHandler userHandler) //bytes inutile mais necessaire pour etre mis dans le dico
        {
            byte[] bytesGame = Serialization.SerializationMatchStatus(userHandler.Game);
            byte[] response = serializationMessage(bytesGame, NomCommande.DGB);
            Messaging.WriteLog(userHandler, $"*** SendGameBoard: to user id {userHandler.Id}");
            return response;
        }

        public static byte[] TransferMatchRequest(byte[] bytes, UserHandler userHandler)
        {
            int idRecipient = BitConverter.ToInt16(bytes, 0);
            int idSender = userHandler.Id;

            Messaging.WriteLog(userHandler, $"*** TransferMatchRequest: try from {idSender} to {idRecipient}");
            string userNameSender = userHandler.UserName;

            byte[] msg = new byte[0];

            if (userHandler.UsersHandlers.ContainsKey(idRecipient) && userHandler.UsersHandlers[idRecipient].Game == null)
            {
                byte[] senderRequest_bytes = serializationGameRequest(idSender, userNameSender);
                byte[] request_msg = serializationMessage(senderRequest_bytes, NomCommande.MRQ);
                StreamWrite(userHandler.UsersHandlers[idRecipient], request_msg);

                Messaging.WriteLog(userHandler, $"*** TransferMatchRequest: success");
            }
            else
            {
                byte[] msg_bytes = serializationResponseOpponent(idRecipient, false);
                msg = serializationMessage(msg_bytes, NomCommande.RGR);

                if(!userHandler.UsersHandlers.ContainsKey(idRecipient))
                {
                    Messaging.WriteLog(userHandler, $"*** TransferMatchRequest: failed, the key {idRecipient} was not found");
                }
                else
                {
                    Messaging.WriteLog(userHandler, $"*** TransferMatchRequest: failed, the user id {idRecipient} is in a match");
                }
                    
            }
            return msg;
        }

        public static byte[] TransferGameRequestResponse(byte[] bytes, UserHandler userHandler)
        {
            int idSender = userHandler.Id;
            Tuple<int, bool> tuple = deserializationResponseOpponent(bytes);
            int idRecipient = tuple.Item1;
            bool response = tuple.Item2;

            Messaging.WriteLog(userHandler, $"*** TransferGameRequestResponse: from {idSender} to {idRecipient}, accepted = {response}");

            //la réponse est envoyée au destinataire
            byte[] msg_bytes = serializationResponseOpponent(idSender, response);
            byte[] msg_to_dest = serializationMessage(msg_bytes, NomCommande.RGR);
            StreamWrite(userHandler.UsersHandlers[idRecipient], msg_to_dest);

            //la réponse est confirmée à l'envoyeur
            msg_bytes = serializationResponseOpponent(idRecipient, response);
            byte[] msg_to_sender = serializationMessage(msg_bytes, NomCommande.RGR);
            StreamWrite(userHandler, msg_to_sender);

            if (response) //creation de l'objet game
            {
                Game game = new Game();
                game.SpecifyPlayersID(idSender, idRecipient);
                userHandler.Game = game;
                userHandler.UsersHandlers[idRecipient].Game = game;
                userHandler.UsersHandlers[idSender].Game = game;

                Messaging.WriteLog(userHandler, $"*** TransferGameRequestResponse: game object created");

                //on envoie la board au destinataire
                byte[] msg_board1 = SendGameBoard(new byte[0], userHandler.UsersHandlers[idRecipient]);
                StreamWrite(userHandler.UsersHandlers[idRecipient], msg_board1);

                //on envoie la board à l'envoyeur
                byte[] msg_board2 = SendGameBoard(new byte[0], userHandler);
                StreamWrite(userHandler, msg_board2);
            }

            return new byte[0];
        }
        
        
        public static void SendMessage(UserHandler userHandler, string message)
        {

            var cmd = Encoding.UTF8.GetBytes(NomCommande.MSG.ToString());
            var message_length = BitConverter.GetBytes((Int16)message.Length);
            var message_bytes = Encoding.UTF8.GetBytes(message);


            byte[] msg = new byte[cmd.Length + message_length.Length + message_bytes.Length];

            
            cmd.CopyTo(msg, 0);
            message_length.CopyTo(msg, cmd.Length);
            message_bytes.CopyTo(msg, cmd.Length + message_length.Length);
            StreamWrite(userHandler, msg);

        }

        public static void WriteLog(string logFile, Mutex logMutex, string log)
        {
            DateTime localDate = DateTime.Now;
            string log_date = localDate.ToString("s");

            logMutex.WaitOne();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(logFile, true))
            {
                file.WriteLine(log_date + " " + log);
            }
            logMutex.ReleaseMutex();
        }

        public static void WriteLog(UserHandler userHandler, string log)
        {
            DateTime localDate = DateTime.Now;
            string log_date = localDate.ToString("s");

            userHandler.LogMutex.WaitOne();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(userHandler.LogFile, true))
            {
                file.WriteLine(log_date + " " + log);
            }
            userHandler.LogMutex.ReleaseMutex();
        }
        
    }
}
