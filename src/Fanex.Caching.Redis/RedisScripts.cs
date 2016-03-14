namespace Fanex.Caching.Redis
{
    internal static class RedisScripts
    {
        public const string GetValue =
            @"local cachedData = redis.call('HGET', KEYS[1], 'data')
              local slidingExpiration = redis.call('HGET', KEYS[1], 'sliding-expiration')

              if slidingExpiration ~= false then
                  redis.call('EXPIRE', KEYS[1], slidingExpiration)
              end

              if cachedData ~= false then
                  return cachedData
              else
                  return ''
              end";

        public const string SetValueWithAbsoluteExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])
              redis.call('EXPIREAT', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithRelativeExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])
              redis.call('EXPIRE', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithSlidingExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])
              redis.call('HMSET', KEYS[1], 'sliding-expiration', ARGV[2])
              redis.call('EXPIRE', KEYS[1], ARGV[2])

              return 1";
    }
}