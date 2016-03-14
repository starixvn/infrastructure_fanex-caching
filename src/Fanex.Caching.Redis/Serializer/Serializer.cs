namespace Fanex.Caching.Redis
{
    public interface Serializer
    {
        byte[] Serialize(object obj);

        object Deserialize(byte[] binaryData);
    }
}