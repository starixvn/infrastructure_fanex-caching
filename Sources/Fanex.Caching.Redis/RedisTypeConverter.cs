namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

#pragma warning disable S1541 // Methods and properties should not be too complex

    internal static class RedisTypeConverter
    {
        public static RedisValue ToRedisValue(object value, ISerializer serializer)
        {
            if (typeof(byte[]) == value.GetType())
            {
                return (byte[])value;
            }

            return serializer.Serialize(value);
        }

        public static T ToObject<T>(RedisResult value, ISerializer serializer)
        {
            var binary = (byte[])value;

            if (binary is T resultInBinary)
            {
                return resultInBinary;
            }

            return serializer.Deserialize<T>(binary);
        }
    }
}