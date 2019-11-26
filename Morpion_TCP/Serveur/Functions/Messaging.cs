using Serveur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Serveur.ModelGame;

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

        private static byte[] serializationGameRequest(int id, string userName)
        {
            byte[] user_id_bytes = BitConverter.GetBytes((Int16)id);
            byte[] userName_bytes = Encoding.UTF8.GetBytes(userName);
            byte[] userName_lenght = BitConverter.GetBytes((Int16)userName_bytes.Length);

            byte[] message_bytes = new byte[user_id_bytes.Length + userName_lenght.Length + userName_bytes.Length];

            //command
            user_id_bytes.CopyTo(message_bytes, 0);
            //length to follow
            userName_lenght.CopyTo(message_bytes, user_id_bytes.Length);
            //content
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
            string userName = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            userHandler.UserName = userName;
            //Console.WriteLine($" >> message recieved from client Id {userHandler.Id} its new userName: {userHandler.UserName}");
            return new byte[0];
        }

        public static byte[] RecieveMessage(byte[] bytes, UserHandler userHandler)
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($" >> message recieved from client {userHandler.UserName} Id {userHandler.Id} : {message}");
            return new byte[0];
        }

        public static byte[] RecievePing(byte[] bytes, UserHandler userHandler)
        {
            Console.WriteLine($" >> ping recieved from client {userHandler.UserName} Id {userHandler.Id}");
            return new byte[0];
        }

        public static byte[] SendOtherUsers(byte[] bytes, UserHandler userHandler)
        {
            
            var bytes_users = from e in userHandler.UsersHandlers.Values
                        where e.Id != userHandler.Id && e.IsAlive()
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
            //Console.WriteLine($" >> The client {userHandler.UserName} Id {userHandler.Id} asked for all connected user");
            //Console.WriteLine($" >> packet sent {cmd_string}");
            return response;
        }

        public static byte[] ReceivePositionPlayed(byte[] bytes, UserHandler userHandler)
        {
            Messaging.WriteLog(userHandler.log_file, $"*** ReceivePositionPlayed: from user id {userHandler.Id}");
            Vector3 position = Serialization.DeserializationPositionPlayed(bytes);
            //Console.WriteLine($"l'identifiant du joueur 1 est : {userHandler.Game.IdPlayer1}");
            //Console.WriteLine($"l'identifiant du joueur 2 est : {userHandler.Game.IdPlayer2}");
            //Console.WriteLine($"le mode du jeu est : {userHandler.Game.Mode}");
            if (userHandler.Game.Play(position, userHandler.Id))
            {
                Messaging.WriteLog(userHandler.log_file, $"*** ReceivePositionPlayed: success");
                //on renvoie la board actualisée
                byte[] msg_board1 = SendGameBoard(new byte[0], userHandler.UsersHandlers[userHandler.Game.IdPlayer1]);
                userHandler.UsersHandlers[userHandler.Game.IdPlayer1].stream.Write(msg_board1, 0, msg_board1.Length);

                byte[] msg_board2 = SendGameBoard(new byte[0], userHandler.UsersHandlers[userHandler.Game.IdPlayer2]);
                userHandler.UsersHandlers[userHandler.Game.IdPlayer2].stream.Write(msg_board2, 0, msg_board1.Length);
            }
            else
            {
                Messaging.WriteLog(userHandler.log_file, $"*** ReceivePositionPlayed: failed, illegal move");
            }
            //Console.WriteLine($"La position a ete jouee");
            //Console.WriteLine($"l'identifiant du joueur 1 est : {userHandler.Game.IdPlayer1}");
            //Console.WriteLine($"l'identifiant du joueur 2 est : {userHandler.Game.IdPlayer2}");
            //Console.WriteLine($"le mode du jeu est : {userHandler.Game.Mode}");
            return new byte[0];
        }

        public static byte[] SendGameBoard(byte[] bytes, UserHandler userHandler) //bytes inutile mais necessaire pour etre mis dans le dico
        {
            byte[] bytesGame = Serialization.SerializationMatchStatus(userHandler.Game);
            /*if (!(userHandler.Game.Mode == GameMode.Player1 || userHandler.Game.Mode == GameMode.Player2))
            {
                userHandler.Game = null;
            }*/
            // le jeu ne doit pas etre detruit si le mode est autre que Player 1 ou Player 2 car unity doit pouvoir afficher player1Won ou player2won
            byte[] response = serializationMessage(bytesGame, NomCommande.DGB);
            Messaging.WriteLog(userHandler.log_file, $"*** SendGameBoard: to user id {userHandler.Id}");
            return response;
        }

        public static byte[] TransferMatchRequest(byte[] bytes, UserHandler userHandler)
        {
            int idRecipient = BitConverter.ToInt16(bytes, 0);
            int idSender = userHandler.Id;

            Messaging.WriteLog(userHandler.log_file, $"*** TransferMatchRequest: try from {idSender} to {idRecipient}");
            string userNameSender = userHandler.UserName;

            byte[] msg = new byte[0];

            

            if (userHandler.UsersHandlers.ContainsKey(idRecipient) && userHandler.UsersHandlers[idRecipient].Game == null)
            {
                byte[] senderRequest_bytes = serializationGameRequest(idSender, userNameSender);
                byte[] request_msg = serializationMessage(senderRequest_bytes, NomCommande.MRQ);
                userHandler.UsersHandlers[idRecipient].stream.Write(request_msg, 0, request_msg.Length);
                Messaging.WriteLog(userHandler.log_file, $"*** TransferMatchRequest: success");
            }
            else
            {
                // on rempli msg d'un refus de requête
                byte[] msg_bytes = serializationResponseOpponent(idRecipient, false);
                msg = serializationMessage(msg_bytes, NomCommande.RGR);

                if(!userHandler.UsersHandlers.ContainsKey(idRecipient))
                {
                    Messaging.WriteLog(userHandler.log_file, $"*** TransferMatchRequest: failed, the key {idRecipient} was not found");
                }
                else
                {
                    Messaging.WriteLog(userHandler.log_file, $"*** TransferMatchRequest: failed, the user id {idRecipient} is in a match");
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

            Messaging.WriteLog(userHandler.log_file, $"*** TransferGameRequestResponse: from {idSender} to {idRecipient}, accepted = {response}");

            //la réponse est envoyée au destinataire
            byte[] msg_bytes = serializationResponseOpponent(idSender, response);
            byte[] msg_to_dest = serializationMessage(msg_bytes, NomCommande.RGR);
            userHandler.UsersHandlers[idRecipient].stream.Write(msg_to_dest, 0, msg_to_dest.Length);

            //la réponse est confirmée à l'envoyeur
            msg_bytes = serializationResponseOpponent(idRecipient, response);
            byte[] msg_to_sender = serializationMessage(msg_bytes, NomCommande.RGR);
            userHandler.stream.Write(msg_to_sender, 0, msg_to_sender.Length);

            if (response) //creation de l'objet game
            {
                Game game = new Game();
                game.SpecifyPlayersID(idSender, idRecipient);
                userHandler.Game = game;
                userHandler.UsersHandlers[idRecipient].Game = game;
                userHandler.UsersHandlers[idSender].Game = game;

                Messaging.WriteLog(userHandler.log_file, $"*** TransferGameRequestResponse: game object created");

                //on envoie la board au destinataire
                byte[] msg_board1 = SendGameBoard(new byte[0], userHandler.UsersHandlers[idRecipient]);
                userHandler.UsersHandlers[idRecipient].stream.Write(msg_board1, 0, msg_board1.Length);

                //on envoie la board à l'envoyeur
                byte[] msg_board2 = SendGameBoard(new byte[0], userHandler);
                userHandler.stream.Write(msg_board2, 0, msg_board2.Length);
            }

            return new byte[0];
        }

        public static void SendNotifcationDisconnection(NetworkStream stream, UserHandler userHandler)
        {
            int idSender = userHandler.Id;
            int idRecipient = (userHandler.Id == userHandler.Game.IdPlayer1)? userHandler.Game.IdPlayer2 : userHandler.Game.IdPlayer1;
            byte[] msg_serialized=serializationMessage(new byte[0], NomCommande.NDC);
            Messaging.WriteLog(userHandler.log_file, $"*** SendNotifcationDisconnection: try from {idSender} to {idRecipient}");
            userHandler.UsersHandlers[idRecipient].stream.Write(msg_serialized, 0, msg_serialized.Length);
            Messaging.WriteLog(userHandler.log_file, $"*** SendNotifcationDisconnection: success");
        }
        

        // A supprimer !
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

        public static void WriteLog(string log_file, string log)
        {
            DateTime localDate = DateTime.Now;
            string log_date = localDate.ToString("s");
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(log_file, true))
            {
                file.WriteLine(log_date + " " + log);
            }
        }
        
    }
}
