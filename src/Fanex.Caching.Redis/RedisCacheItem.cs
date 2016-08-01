namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

    internal class RedisCacheItem : CacheItem
    {
        private Serializer _serializer;

        public RedisCacheItem(object key, object value, CacheItemOptions options, Serializer serializer)
            : base(key, value, options)
        {
            _serializer = serializer;
        }

        public RedisCommand ToGetCommand()
        {
            var command = new RedisCommand();

            command.Script = RedisScripts.GetValue;
            command.Keys = new RedisKey[] { (string)Key };

            return command;
        }

        public RedisCommand ToSetCommand()
        {
            if (Options.AbsoluteExpiration.HasValue)
            {
                return ToSetCommandWithAbsoluteExpiration();
            }
            else if (Options.SlidingExpiration.HasValue)
            {
                return ToSetCommandWithSlidingExpiration();
            }

            return ToSetCommandWithRelativeExpiration();
        }

        private RedisCommand ToSetCommandWithAbsoluteExpiration()
        {
            var command = new RedisCommand();

            command.Script = RedisScripts.SetValueWithAbsoluteExpiration;
            command.Keys = new RedisKey[] { (string)Key };
            command.Values = new RedisValue[]
            {
                RedisValueHelper.ConvertToRedisValue(Value, _serializer),
#if NET452 || NET40
                ToUnixTimeSeconds(Options.AbsoluteExpiration.Value)
#else
                Options.AbsoluteExpiration?.ToUnixTimeSeconds()
#endif
            };

            return command;
        }

        private RedisCommand ToSetCommandWithRelativeExpiration()
        {
            var command = new RedisCommand();

            Options.RelativeExpiration = Options.RelativeExpiration ?? CacheItemOptions.DefaultRelativeExpiration;

            command.Script = RedisScripts.SetValueWithRelativeExpiration;
            command.Keys = new RedisKey[] { (string)Key };
            command.Values = new RedisValue[]
            {
                RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                Options.RelativeExpiration?.TotalSeconds
            };

            return command;
        }

        private RedisCommand ToSetCommandWithSlidingExpiration()
        {
            var command = new RedisCommand();

            command.Script = RedisScripts.SetValueWithSlidingExpiration;
            command.Keys = new RedisKey[] { (string)Key };
            command.Values = new RedisValue[]
            {
                RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                Options.SlidingExpiration?.TotalSeconds
            };

            return command;
        }

#if NET452 || NET40

        private static long ToUnixTimeSeconds(DateTimeOffset absoluteDateTime)
        {
            var epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0));

            return (long)(absoluteDateTime.UtcDateTime - epochTime.UtcDateTime).TotalSeconds;
        }

#endif
    }
}