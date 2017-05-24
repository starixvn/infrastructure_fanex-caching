namespace Fanex.Caching.Redis
{
    using System;

    public interface Serializer
    {
        byte[] Serialize(object obj);

        object Deserialize(byte[] binaryData, Type expectedType);
    }
}