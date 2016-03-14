namespace Fanex.Caching.Redis
{
    internal static class RedisCommands
    {
        public const string GetValueAndStatusScript =
            @"local retArray = {}
              local cachedData = redis.call('HGET', KEYS[1], 'Data')
              local status = redis.call('HGET', KEYS[1], 'Status')
              local slidingTimeout = redis.call('HGET', KEYS[1], 'SlidingTimeout')

              if slidingTimeout ~= false then
                  redis.call('EXPIRE', KEYS[1], slidingTimeout)
              end

              if cachedData ~= false then
                  retArray[1] = cachedData
              else
                  retArray[1] = ''
              end

              if cachedData ~= false then
                  retArray[2] = status
              else
                  retArray[2] = false
              end

              return retArray";

        public const string SetValueWithTimeoutScript =
            @"redis.call('HMSET', KEYS[1], 'Data', ARGV[1])
              redis.call('HMSET', KEYS[1], 'Status', ARGV[2])
              redis.call('EXPIRE', KEYS[1], ARGV[3])

              return 1";

        public const string SetValueWithSlidingTimeoutScript =
            @"redis.call('HMSET', KEYS[1], 'Data', ARGV[1])
              redis.call('HMSET', KEYS[1], 'Status', ARGV[2])
              redis.call('HMSET', KEYS[1], 'SlidingTimeout', ARGV[3])
              redis.call('EXPIRE', KEYS[1], ARGV[3])

              return 1";

        public const string SetValueWithAbsoluteTimeoutScript =
            @"redis.call('HMSET', KEYS[1], 'Data', ARGV[1])
              redis.call('HMSET', KEYS[1], 'Status', ARGV[2])
              redis.call('EXPIREAT', KEYS[1], ARGV[3])

              return 1";
    }
}