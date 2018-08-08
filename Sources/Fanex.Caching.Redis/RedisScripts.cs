namespace Fanex.Caching.Redis
{
    internal static class RedisScripts
    {
        public const string GetValue =
            @"local fields = redis.call('HMGET', KEYS[1], 'data', 'sldExpr', 'relExpr')
              local cachingData = fields[1]
              local slidingExpiration = fields[2]
              local relativeExpiration = fields[3]

              if slidingExpiration ~= false then
                  local newTimeToLive = slidingExpiration

                  if relativeExpiration ~= false then
                      local restTimeToLive = redis.call('PTTL', KEYS[1])
                      local newRelativeExpiration = relativeExpiration - (slidingExpiration - restTimeToLive)

                      redis.call('HSET', KEYS[1], 'relExpr', newRelativeExpiration)

                      if newRelativeExpiration < newTimeToLive then
                          newTimeToLive = relativeExpiration
                      end
                  end

                  redis.call('PEXPIRE', KEYS[1], newTimeToLive)
              end

              return cachingData";

        public const string SetValueWithSlidingAndRelativeExpiration =
            @"if ARGV[3] < ARGV[2] then
                  redis.call('HSET', KEYS[1], 'data', ARGV[1])
                  redis.call('PEXPIRE', KEYS[1], ARGV[3])
              else
                  redis.call('HMSET', KEYS[1], 'data', ARGV[1], 'sldExpr', ARGV[2], 'relExpr', ARGV[3])
                  redis.call('PEXPIRE', KEYS[1], ARGV[2])
              end

              return 1";

        public const string SetValueWithSlidingExpiration =
            @"redis.call('HMSET', KEYS[1], 'data', ARGV[1], 'sldExpr', ARGV[2])
              redis.call('PEXPIRE', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithRelativeExpiration =
            @"redis.call('HSET', KEYS[1], 'data', ARGV[1])
              redis.call('PEXPIRE', KEYS[1], ARGV[2])

              return 1";

        public const string SetValueWithoutExpiration =
            @"redis.call('HSET', KEYS[1], 'data', ARGV[1])

              return 1";
    }
}