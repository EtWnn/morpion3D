using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Client.ModelGame
{
    static class Serialization
    {
        public static byte[] SerializationMatchStatus(Game match)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Game));
            MemoryStream mems = new System.IO.MemoryStream();
            serializer.Serialize(mems, match);
            string _str = Encoding.UTF8.GetString(mems.ToArray());
            Byte[] data = Encoding.UTF8.GetBytes(_str);
            return data;
        }

        public static Game DeserializationMatchStatus(Byte[] data)
        {
            string _strg = Encoding.UTF8.GetString(data);
            Game match1 = null;
            XmlSerializer serializer = new XmlSerializer(typeof(Game));
            XmlReader xr = XmlReader.Create(new StringReader(_strg));
            if (serializer.CanDeserialize(xr))
            {
                match1 = (Game)serializer.Deserialize(xr);
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
