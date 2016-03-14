namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    internal class RedisCacheItem : CacheItem
    {
        private Serializer _serializer;

        public RedisCacheItem(object key, object value, CacheItemOptions options, Serializer serializer)
            : base(key, value, options)
        {
            _serializer = serializer;
        }

        public RedisCommand GetGetCommand()
        {
            var command = new RedisCommand();

            command.Script = RedisScripts.GetValue;
            command.Keys = new RedisKey[] { (string)Key };

            return command;
        }

        public RedisCommand GetSetCommand()
        {
            if (Options.AbsoluteExpiration.HasValue)
            {
                return GetSetCommandWithAbsoluteExpiration();
            }
            else if (Options.SlidingExpiration.HasValue)
            {
                return GetSetCommandWithSlidingExpiration();
            }

            return GetSetCommandWithRelativeExpiration();
        }

        private RedisCommand GetSetCommandWithAbsoluteExpiration()
        {
            var command = new RedisCommand();

            command.Script = RedisScripts.SetValueWithAbsoluteExpiration;
            command.Keys = new RedisKey[] { (string)Key };
            command.Values = new RedisValue[]
            {
                RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                Options.AbsoluteExpiration?.ToString(RedisValueHelper.DateTimeFormat)
            };

            return command;
        }

        private RedisCommand GetSetCommandWithRelativeExpiration()
        {
            var command = new RedisCommand();

            command.Script = RedisScripts.SetValueWithRelativeExpiration;
            command.Keys = new RedisKey[] { (string)Key };
            command.Values = new RedisValue[]
            {
                RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                Options.RelativeExpiration?.TotalSeconds
            };

            return command;
        }

        private RedisCommand GetSetCommandWithSlidingExpiration()
        {
            var command = new RedisCommand();

            Options.SlidingExpiration = Options.SlidingExpiration ?? CacheItemOptions.DefaultRelativeExpiration;

            command.Script = RedisScripts.SetValueWithSlidingExpiration;
            command.Keys = new RedisKey[] { (string)Key };
            command.Values = new RedisValue[]
            {
                RedisValueHelper.ConvertToRedisValue(Value, _serializer),
                Options.SlidingExpiration?.TotalSeconds
            };

            return command;
        }
    }
}