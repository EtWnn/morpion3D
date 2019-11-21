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
        RGR
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
            Console.WriteLine($" >> The client {userHandler.UserName} Id {userHandler.Id} asked for all connected user");
            //Console.WriteLine($" >> packet sent {cmd_string}");
            return response;
        }

        public static byte[] ReceivePositionPlayed(byte[] bytes, UserHandler userHandler)
        {
            Vector3 position = Serialization.DeserializationPositionPlayed(bytes);
            Console.WriteLine($"l'identifiant du joueur 1 est : {userHandler.Game.IdPlayer1}");
            Console.WriteLine($"l'identifiant du joueur 2 est : {userHandler.Game.IdPlayer2}");
            Console.WriteLine($"le mode du jeu est : {userHandler.Game.Mode}");
            userHandler.Game.Play(position, userHandler.Id);
            Console.WriteLine($"La position a ete jouee");
            Console.WriteLine($"l'identifiant du joueur 1 est : {userHandler.Game.IdPlayer1}");
            Console.WriteLine($"l'identifiant du joueur 2 est : {userHandler.Game.IdPlayer2}");
            Console.WriteLine($"le mode du jeu est : {userHandler.Game.Mode}");
            return new byte[0];
        }

        public static byte[] SendGameBoard(byte[] bytes, UserHandler userHandler) //bytes inutile mais necessaire pour etre mis dans le dico
        {
            byte[] bytesGame = Serialization.SerializationMatchStatus(userHandler.Game);
            if (!(userHandler.Game.Mode == GameMode.Player1 || userHandler.Game.Mode == GameMode.Player2))
            {
                userHandler.Game = null;
            }

            byte[] response = serializationMessage(bytesGame, NomCommande.DGB);
            return response;
        }

        public static byte[] TransferMatchRequest(byte[] bytes, UserHandler userHandler)
        {
            //Console.WriteLine($">> Serveur.Messaging TransfertMatchRequest bytes.Lenght : {bytes.Length}");
            int idRecipient = BitConverter.ToInt16(bytes, 0);
            //Console.WriteLine($">> Serveur.Messaging TransfertMatchRequest idRecipient : {idRecipient}");
            int idSender = userHandler.Id;
            //Console.WriteLine($">> Serveur.Messaging TransfertMatchRequest idSender : {idSender}");
            string userNameSender = userHandler.UserName;
            byte[] senderRequest_bytes = serializationGameRequest(idSender, userNameSender);
            byte[] msg = serializationMessage(senderRequest_bytes, NomCommande.MRQ);
            userHandler.UsersHandlers[idRecipient].stream.Write(msg, 0, msg.Length);
            return new byte[0];
        }

        public static byte[] TransferGameRequestResponse(byte[] bytes, UserHandler userHandler)
        {
            int idSender = userHandler.Id;
            Tuple<int, bool> tuple = deserializationResponseOpponent(bytes);
            int idRecipient = tuple.Item1;
            Console.WriteLine($">> idSender = userHandler.Id est {idSender}");
            Console.WriteLine($">> idRecipient est {idRecipient}");
            bool response = tuple.Item2;
            byte[] msg = new byte[0];
            if (response)
            {
                byte[] msg_bytes = serializationResponseOpponent(idSender, response);
                msg = serializationMessage(msg_bytes, NomCommande.RGR);
                Console.WriteLine($"La longueur du msg envoyé à {idRecipient} est {msg.Length}");
                userHandler.UsersHandlers[idRecipient].stream.Write(msg, 0, msg.Length);

                Game game = new Game();
                game.SpecifyPlayersID(idSender, idRecipient);
                Console.WriteLine($"l'id du player 1 est : {game.IdPlayer1}");
                Console.WriteLine($"l'id du player 2 est : {game.IdPlayer2}");
                userHandler.Game = game;
                userHandler.UsersHandlers[idRecipient].Game = game;

                msg_bytes = serializationResponseOpponent(idRecipient, response);
                msg = serializationMessage(msg_bytes, NomCommande.RGR);
                Console.WriteLine($"La longueur du msg envoyé à {idSender} est {msg.Length}");
            }
            else
            {
                
            }
            return msg;
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
        
    }
}
