using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Numerics;

namespace regleJeu
{
    static class Serialize
    {
        public static byte[] SerializationMatchStatus(Match match)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Match));
            MemoryStream mems = new System.IO.MemoryStream();
            serializer.Serialize(mems, match);
            string _str = Encoding.UTF8.GetString(mems.ToArray());
            Byte[] data = Encoding.UTF8.GetBytes(_str);
            return data;
        }

        public static Match DeserializationMatchStatus(Byte[] data)
        {
            string _strg = Encoding.UTF8.GetString(data);
            Match match1 = null;
            XmlSerializer serializer = new XmlSerializer(typeof(Match));
            XmlReader xr = XmlReader.Create(new StringReader(_strg));
            if (serializer.CanDeserialize(xr))
            {
                match1 = (Match)serializer.Deserialize(xr);
            }
            return match1;
        }

        public static byte[] SerializationPositionPlayed(Vector3 position)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Vector3));
            MemoryStream mems = new System.IO.MemoryStream();
            serializer.Serialize(mems, position);
            string _str = Encoding.UTF8.GetString(mems.ToArray());
            Byte[] data = Encoding.UTF8.GetBytes(_str);
            return data;
        }

        public static Vector3 DeserializationPositionPlayed(Byte[] data)
        {
            string _strg = Encoding.UTF8.GetString(data);
            Vector3 position = new Vector3();
            XmlSerializer serializer = new XmlSerializer(typeof(Vector3));
            XmlReader xr = XmlReader.Create(new StringReader(_strg));
            if (serializer.CanDeserialize(xr))
            {
                position = (Vector3)serializer.Deserialize(xr);
            }
            return position;
        }
    }
}
