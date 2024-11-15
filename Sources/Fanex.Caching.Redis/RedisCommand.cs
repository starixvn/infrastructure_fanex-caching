namespace Fanex.Caching.Redis;

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
        => new RedisCommand(RedisScripts.GetValue, key, null);

    public static RedisCommand CreateSetCommand(
        RedisKey key,
        RedisValue value,
        CacheItemOptions options,
        DateTimeOffset createdTime)
    {
        var timeToLive = options.CalculateRelativeExpiration(createdTime);
        var timeToSlide = options.SlidingExpiration;

        if (timeToLive.HasValue)
        {
            if (timeToLive > timeToSlide)
            {
                return new RedisCommand(
                    RedisScripts.SetValueWithSlidingAndRelativeExpiration,
                    key,
                    value,
                    CalculateTotalMilliseconds(timeToSlide.Value),
                    CalculateTotalMilliseconds(timeToLive.Value));
            }

            return new RedisCommand(
                RedisScripts.SetValueWithRelativeExpiration,
                key,
                value,
                CalculateTotalMilliseconds(timeToLive.Value));
        }

        if (timeToSlide.HasValue)
        {
            return new RedisCommand(
                RedisScripts.SetValueWithSlidingExpiration,
                key,
                value,
                CalculateTotalMilliseconds(timeToSlide.Value));
        }

        return new RedisCommand(
            RedisScripts.SetValueWithoutExpiration,
            key,
            value);
    }

    private static long CalculateTotalMilliseconds(TimeSpan expiration)
        => expiration.Ticks / TimeSpan.TicksPerMillisecond;
}