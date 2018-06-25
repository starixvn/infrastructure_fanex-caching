namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    internal class RedisCacheItem
    {
        public RedisCacheItem(RedisKey key, RedisValue value, CacheItemOptions options)
        {
            Key = key;
            Value = value;
            Options = options;
        }

        public RedisKey Key { get; }

        public RedisValue Value { get; }

        public CacheItemOptions Options { get; }

        public RedisCommand ToGetCommand()
            => new RedisCommand
            {
                Script = RedisScripts.GetValue,
                Keys = new[] { Key }
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
                Keys = new[] { Key },
                Values = new[]
                {
                    Value,
                    Options.AbsoluteExpiration?.ToUnixTimeSeconds()
                }
            };

        private RedisCommand ToSetCommandWithRelativeExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithRelativeExpiration,
                Keys = new[] { Key },
                Values = new[]
                {
                    Value,
                    Options.RelativeExpiration?.TotalSeconds
                }
            };

        private RedisCommand ToSetCommandWithSlidingExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithSlidingExpiration,
                Keys = new[] { Key },
                Values = new[]
                {
                    Value,
                    Options.SlidingExpiration?.TotalSeconds
                }
            };

        private RedisCommand ToSetCommandWithoutExpiration()
            => new RedisCommand
            {
                Script = RedisScripts.SetValueWithoutExpiration,
                Keys = new[] { Key },
                Values = new[] { Value }
            };
    }
}