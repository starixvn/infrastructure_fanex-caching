namespace Fanex.Caching.Redis
{
    using System;
    using MP = MsgPack.Serialization;

    public class MessagePackSerializer : Serializer
    {
        public object Deserialize(byte[] binaryData, Type expectedType)
        {
            if (binaryData == null)
            {
                throw new ArgumentNullException(nameof(binaryData));
            }

            var serializer = MP.MessagePackSerializer.Get(expectedType);
            object result = serializer.UnpackSingleObject(binaryData);

            return result;
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var serializer = MP.MessagePackSerializer.Get(obj.GetType());
            byte[] result = serializer.PackSingleObject(obj);

            return result;
        }
    }
}