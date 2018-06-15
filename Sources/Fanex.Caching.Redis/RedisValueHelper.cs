namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

#pragma warning disable S1541 // Methods and properties should not be too complex

    internal static class RedisValueHelper
    {
        public static RedisValue ConvertToRedisValue(object value, ISerializer serializer)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type valueType = value.GetType();

            if (typeof(byte[]) == valueType)
            {
                return (byte[])value;
            }
            else if (typeof(bool) == valueType)
            {
                return (bool)value;
            }
            else if (typeof(bool?) == valueType)
            {
                return (bool?)value;
            }
            else if (typeof(int) == valueType)
            {
                return (int)value;
            }
            else if (typeof(int?) == valueType)
            {
                return (int?)value;
            }
            else if (typeof(long) == valueType)
            {
                return (long)value;
            }
            else if (typeof(long?) == valueType)
            {
                return (long?)value;
            }
            else if (typeof(double) == valueType)
            {
                return (double)value;
            }
            else if (typeof(double?) == valueType)
            {
                return (double?)value;
            }
            else if (typeof(string) == valueType)
            {
                return (string)value;
            }
            else
            {
                return serializer.Serialize(value);
            }
        }

        public static object ConvertToObject<T>(RedisResult value, ISerializer serializer)
        {
            Type valueType = typeof(T);

            if (typeof(byte[]) == valueType)
            {
                return (byte[])value;
            }
            else if (typeof(bool) == valueType)
            {
                return (bool)value;
            }
            else if (typeof(bool?) == valueType)
            {
                return (bool?)value;
            }
            else if (typeof(int) == valueType)
            {
                return (int)value;
            }
            else if (typeof(int?) == valueType)
            {
                return (int?)value;
            }
            else if (typeof(long) == valueType)
            {
                return (long)value;
            }
            else if (typeof(long?) == valueType)
            {
                return (long?)value;
            }
            else if (typeof(double) == valueType)
            {
                return (double)value;
            }
            else if (typeof(double?) == valueType)
            {
                return (double?)value;
            }
            else if (typeof(string) == valueType)
            {
                return (string)value;
            }
            else
            {
                return serializer.Deserialize<T>((byte[])value);
            }
        }
    }
}