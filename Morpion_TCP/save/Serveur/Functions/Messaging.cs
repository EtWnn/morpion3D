using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serveur.Functions
{
    class Messaging
    {
        public static byte[] MessageToByte(string message, int max_bytes)
        {
            byte[] msg = new byte[5 + message.Length];
            int i = 0;
            byte[] temp = Encoding.UTF8.GetBytes("MSG");
            foreach (byte e in temp) { msg[i] = e; i++; }

            temp = BitConverter.GetBytes((Int16)message.Length);
            foreach (byte e in temp) { msg[i] = e; i++; }
            temp = Encoding.UTF8.GetBytes(message);
            foreach(byte e in temp) { msg[i] = e; i++; }

            return msg;
        }

        public static string ByteToMessage(byte[] msg)
        {
            int len_content = BitConverter.ToInt16(msg, 3);
            string content = System.Text.Encoding.UTF8.GetString(msg, 5, len_content);
            return content;
        }


    }
}
