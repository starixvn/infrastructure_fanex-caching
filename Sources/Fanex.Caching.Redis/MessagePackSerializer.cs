namespace Fanex.Caching.Redis
{
    using MessagePack.Resolvers;
    using MP = MessagePack;

    public class MessagePackSerializer : ISerializer
    {
        public byte[] Serialize(object obj)
            => MP.MessagePackSerializer.Serialize(obj, ContractlessStandardResolver.Instance);

        public T Deserialize<T>(byte[] binaryData)
        {
            try
            {
                return MP.MessagePackSerializer.Deserialize<T>(binaryData, ContractlessStandardResolver.Instance);
            }
            catch
            {
                return default;
            }
        }
    }
}