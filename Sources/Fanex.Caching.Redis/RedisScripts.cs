namespace Fanex.Caching.Redis;

internal static class RedisScripts
{
    public const string GetValue =
        """
        local fields = redis.call('HMGET', KEYS[1], 'data', 'sldExpr', 'relExpr')
                      local cachingData = fields[1]
                      local slidingExpiration = fields[2]
        
                      if slidingExpiration ~= false then
                          local relativeExpiration = fields[3]
                          local timeToSlide = tonumber(slidingExpiration)
                          local newTimeToLive = timeToSlide
        
                          if relativeExpiration ~= false then
                              local totalTimeToLive = tonumber(relativeExpiration)
                              local restTimeToLive = redis.call('PTTL', KEYS[1])
                              local newRelativeExpiration = totalTimeToLive - (timeToSlide - restTimeToLive)
        
                              redis.call('HSET', KEYS[1], 'relExpr', newRelativeExpiration)
        
                              if newRelativeExpiration < newTimeToLive then
                                  newTimeToLive = newRelativeExpiration
                              end
                          end
        
                          redis.call('PEXPIRE', KEYS[1], newTimeToLive)
                      end
        
                      return cachingData
        """;

    public const string SetValueWithSlidingAndRelativeExpiration =
        """
        redis.call('HMSET', KEYS[1], 'data', ARGV[1], 'sldExpr', ARGV[2], 'relExpr', ARGV[3])
                      redis.call('PEXPIRE', KEYS[1], ARGV[2])
        
                      return 1
        """;

    public const string SetValueWithSlidingExpiration =
        """
        redis.call('HMSET', KEYS[1], 'data', ARGV[1], 'sldExpr', ARGV[2])
                      redis.call('PEXPIRE', KEYS[1], ARGV[2])
        
                      return 1
        """;

    public const string SetValueWithRelativeExpiration =
        """
        redis.call('HSET', KEYS[1], 'data', ARGV[1])
                      redis.call('PEXPIRE', KEYS[1], ARGV[2])
        
                      return 1
        """;

    public const string SetValueWithoutExpiration =
        """
        redis.call('HSET', KEYS[1], 'data', ARGV[1])
        
                      return 1
        """;
}