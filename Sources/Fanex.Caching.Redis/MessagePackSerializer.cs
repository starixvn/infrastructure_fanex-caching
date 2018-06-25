namespace Fanex.Caching.Redis
{
    using MessagePack.Resolvers;
    using MP = MessagePack;

    public class MessagePackSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] binaryData)
            => MP.MessagePackSerializer.Deserialize<T>(binaryData, ContractlessStandardResolver.Instance);

        public byte[] Serialize(object obj)
            => MP.MessagePackSerializer.Serialize(obj, ContractlessStandardResolver.Instance);
    }
}