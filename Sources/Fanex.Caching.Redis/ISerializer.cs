namespace Fanex.Caching.Redis
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);

        T Deserialize<T>(byte[] binaryData);
    }
}