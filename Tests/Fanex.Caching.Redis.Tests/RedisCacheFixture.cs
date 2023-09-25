namespace Fanex.Caching.Redis.Tests;

using System;

public sealed class RedisCacheFixture : IDisposable
{
    public RedisCacheFixture()
    {
        CacheOptions = new RedisCacheOptions
        {
            ConnectionString = "10.18.200.131:16379,password=foo1@bared,defaultDatabase=11",
            ApplicationName = "Fanex.Caching.Redis.Tests"
        };
        Cache = new RedisCache(CacheOptions);
    }

    public RedisCacheOptions CacheOptions { get; }

    public RedisCache Cache { get; private set; }

    public void Dispose()
    {
        Cache.Dispose();
        Cache = null;
    }
}