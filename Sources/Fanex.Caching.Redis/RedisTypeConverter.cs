namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

#pragma warning disable S1541 // Methods and properties should not be too complex

    internal static class RedisTypeConverter
    {
        public static RedisValue ToRedisValue(object value, ISerializer serializer)
        {
            Type valueType = value.GetType();

            if (typeof(byte[]) == valueType)
            {
                return (byte[])value;
            }

            return serializer.Serialize(value);
        }

        public static object ToObject<T>(RedisResult value, ISerializer serializer)
        {
            Type valueType = typeof(T);

            if (typeof(byte[]) == valueType)
            {
                return (byte[])value;
            }

            return serializer.Deserialize<T>((byte[])value);
        }
    }
}