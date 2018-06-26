namespace Fanex.Caching.Redis
{
    internal static class RedisScripts
    {
        public const string GetValue =
            @"local cachingData = redis.call('HGET', KEYS[1], 'data')
              local slidingTime = redis.call('HGET', KEYS[1], 'slidingTime')

              if slidingTime ~= false then
                  redis.call('PEXPIRE', KEYS[1], slidingTime)
              end

              return cachingData";

        public const string SetValueWithAbsoluteExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])
              redis.call('PEXPIREAT', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithRelativeExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])
              redis.call('PEXPIRE', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithSlidingExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])
              redis.call('HMSET', KEYS[1], 'slidingTime', ARGV[2])
              redis.call('PEXPIRE', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithoutExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1])

              return 1";
    }
}