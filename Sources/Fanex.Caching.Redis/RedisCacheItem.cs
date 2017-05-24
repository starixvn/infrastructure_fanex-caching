namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    internal class RedisCacheItem : CacheItem
    {
        private readonly Serializer _serializer;

        public RedisCacheItem(object key, object value, CacheItemOptions options, Serializer serializer)
            : base(key, value, options)
            => _serializer = serializer;

        public RedisCommand ToGetCommand()
            => new RedisCommand()
            {
                Script = RedisScripts.GetValue,
                Keys = new RedisKey[] { Key as string }
            };

        public RedisCommand ToSetCommand()
        {
            if (Options.AbsoluteExpiration.HasValue)
            {
                return ToSetCommandWithAbsoluteExpiration();
            }

            if (Options.SlidingExpiration.HasValue)
            {
                return ToSetCommandWithSlidingExpiration();
            }

            return ToSetCommandWithRelativeExpiration();
        }

        private RedisCommand ToSetCommandWithAbsoluteExpiration()
            => new RedisCommand()
            {
                Script = RedisScripts.SetValueWithAbsoluteExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new RedisValue[]
                {
                    RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                    Options.AbsoluteExpiration?.ToUnixTimeSeconds()
                }
            };

        private RedisCommand ToSetCommandWithRelativeExpiration()
        {
            Options.RelativeExpiration = Options.RelativeExpiration ?? CacheItemOptions.DefaultRelativeExpiration;

            return new RedisCommand()
            {
                Script = RedisScripts.SetValueWithRelativeExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new RedisValue[]
                {
                    RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                    Options.RelativeExpiration?.TotalSeconds
                }
            };
        }

        private RedisCommand ToSetCommandWithSlidingExpiration()
            => new RedisCommand()
            {
                Script = RedisScripts.SetValueWithSlidingExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new RedisValue[]
                {
                    RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                    Options.SlidingExpiration?.TotalSeconds
                }
            };
    }
}