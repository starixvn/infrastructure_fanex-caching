namespace Fanex.Caching.Redis
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class BinarySerializer : Serializer
    {
        public object Deserialize(byte[] binaryData)
        {
            if (binaryData == null)
            {
                throw new ArgumentNullException(nameof(binaryData));
            }

            using (var memoryStream = new MemoryStream(binaryData))
            {
                object result = null;
                var binaryFormatter = new BinaryFormatter();

                memoryStream.Seek(0, SeekOrigin.Begin);
                result = binaryFormatter.Deserialize(memoryStream);
                memoryStream.Flush();

                return result;
            }
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            using (var memoryStream = new MemoryStream())
            {
                byte[] result = null;
                var binaryFormatter = new BinaryFormatter();

                binaryFormatter.Serialize(memoryStream, obj);
                memoryStream.Seek(0, SeekOrigin.Begin);
                result = memoryStream.ToArray();
                memoryStream.Flush();

                return result;
            }
        }
    }
}