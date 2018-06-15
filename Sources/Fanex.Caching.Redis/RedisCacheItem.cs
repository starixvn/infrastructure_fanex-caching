namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    internal class RedisCacheItem : CacheItem
    {
        private readonly RedisValue _redisValue;

        public RedisCacheItem(object key, RedisValue redisValue, CacheItemOptions options)
            : base(key, redisValue, options)
            => _redisValue = redisValue;

        public RedisCommand ToGetCommand()
            => new RedisCommand
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

            if (Options.RelativeExpiration.HasValue)
            {
                return ToSetCommandWithRelativeExpiration();
            }

            if (Options.SlidingExpiration.HasValue)
            {
                return ToSetCommandWithSlidingExpiration();
            }

            return ToSetCommandWithoutExpiration();
        }

        private RedisCommand ToSetCommandWithAbsoluteExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithAbsoluteExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new[]
                {
                    _redisValue,
                    Options.AbsoluteExpiration?.ToUnixTimeSeconds()
                }
            };

        private RedisCommand ToSetCommandWithRelativeExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithRelativeExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new[]
                {
                    _redisValue,
                    Options.RelativeExpiration?.TotalSeconds
                }
            };

        private RedisCommand ToSetCommandWithSlidingExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithSlidingExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new[]
                {
                    _redisValue,
                    Options.SlidingExpiration?.TotalSeconds
                }
            };

        private RedisCommand ToSetCommandWithoutExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithoutExpiration,
                Keys = new RedisKey[] { Key as string },
                Values = new[] { _redisValue }
            };
    }
}