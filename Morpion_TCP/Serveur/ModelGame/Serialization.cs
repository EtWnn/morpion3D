using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Serveur.ModelGame
{
    /// <summary>
    /// <para>Group the methods to serialized and deserialization the objects of a Game : </para>
    /// <para>- 1 - a <see cref="Game"/> </para>
    /// <para>- 2 - a <see cref="Vector3"/> describing a position played </para>
    /// </summary>
    static class Serialization
    {
        // ---- Public Static methods for serialization ----

        /// <summary>
        /// Transform an object <see cref="Game"/> <paramref name="match"/> in a list of <see cref="byte"/>
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static byte[] SerializationMatchStatus(Game match)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Game));
            MemoryStream mems = new System.IO.MemoryStream();
            serializer.Serialize(mems, match);
            string _str = Encoding.UTF8.GetString(mems.ToArray());
            Byte[] data = Encoding.UTF8.GetBytes(_str);
            return data;
        }

        /// <summary>
        /// Transform an object <see cref="Vector3"/> <paramref name="position"/> in a list of <see cref="byte"/>
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static byte[] SerializationPositionPlayed(Vector3 position)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Vector3));
            MemoryStream mems = new System.IO.MemoryStream();
            serializer.Serialize(mems, position);
            string _str = Encoding.UTF8.GetString(mems.ToArray());
            Byte[] data = Encoding.UTF8.GetBytes(_str);
            return data;
        }

        // ---- Public Static methods for deserialization ----

        /// <summary>
        /// Transform a list of <see cref="byte"/> <paramref name="data"/> in an object <see cref="Game"/>
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// ransform a list of <see cref="byte"/> <paramref name="data"/> in an object <see cref="Vector3"/>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
