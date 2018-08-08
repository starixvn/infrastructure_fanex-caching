namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

    internal class RedisCommand
    {
        private RedisCommand(
            string script,
            RedisKey key,
            params RedisValue[] values)
        {
            Keys = new[] { key };
            Values = values;
            Script = script;
        }

        public string Script { get; }

        public RedisKey[] Keys { get; }

        public RedisValue[] Values { get; }

        public static RedisCommand CreateGetCommand(RedisKey key)
            => new RedisCommand(RedisScripts.GetValue, key, RedisValue.Null);

        public static RedisCommand CreateSetCommand(
            RedisKey key,
            RedisValue value,
            CacheItemOptions options,
            DateTimeOffset createdTime)
        {
            var timeToLive = options.CalculateRelativeExpiration(createdTime);

            if (options.SlidingExpiration.HasValue && timeToLive.HasValue)
            {
                return new RedisCommand(
                    RedisScripts.SetValueWithSlidingAndRelativeExpiration,
                    key,
                    value,
                    options.SlidingExpiration.Value.TotalMilliseconds,
                    timeToLive.Value.TotalMilliseconds);
            }

            if (options.SlidingExpiration.HasValue)
            {
                return new RedisCommand(
                    RedisScripts.SetValueWithSlidingExpiration,
                    key,
                    value,
                    options.SlidingExpiration.Value.TotalMilliseconds);
            }

            if (timeToLive.HasValue)
            {
                return new RedisCommand(
                    RedisScripts.SetValueWithRelativeExpiration,
                    key,
                    value,
                    timeToLive.Value.TotalMilliseconds);
            }

            return new RedisCommand(
                RedisScripts.SetValueWithoutExpiration,
                key,
                value);
        }
    }
}