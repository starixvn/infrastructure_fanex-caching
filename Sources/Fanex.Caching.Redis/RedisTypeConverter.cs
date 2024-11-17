namespace Fanex.Caching.Redis;

using System;
using StackExchange.Redis;

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
        if (typeof(T) == typeof(byte[]))
        {
            return (T)Convert.ChangeType(binary, typeof(T)); 
        }

        return serializer.Deserialize<T>(binary);
    }
}