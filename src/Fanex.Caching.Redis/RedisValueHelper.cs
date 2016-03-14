namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

    internal static class RedisValueHelper
    {
        public static RedisValue ConvertToRedisValue(object value, Serializer serializer)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type valueType = value.GetType();

            if (valueType.IsPrimitive
                || typeof(byte[]) == valueType
                || typeof(string) == valueType)
            {
                return (RedisValue)value;
            }
            else
            {
                return serializer.Serialize(value);
            }
        }

        public static object ConvertToObject(RedisValue value, Type valueType, Serializer serializer)
        {
            if (valueType.IsPrimitive
                || typeof(byte[]) == valueType
                || typeof(string) == valueType)
            {
                return value;
            }
            else
            {
                return serializer.Deserialize(value);
            }
        }
    }
}