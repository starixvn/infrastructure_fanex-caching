namespace Fanex.Caching;

using System;
using Memory;

public static class CacheServiceManager
{
    private static ICache cacheStorage;
    private static Func<ICache> cacheStorageFactory;

    internal static ICache CacheStorage
    {
        get
        {
            cacheStorage ??= cacheStorageFactory != null ? cacheStorageFactory() : new MemoryCache();

            return cacheStorage;
        }
    }

    public static void Use(ICache cache)
    {
        if (cache == null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        cacheStorage?.Dispose();

        cacheStorage = cache;
    }

    public static void Use(Func<ICache> factory)
        => cacheStorageFactory = factory ?? throw new ArgumentNullException(nameof(factory));

    internal static void Reset()
    {
        cacheStorageFactory = null;

        if (cacheStorage == null)
        {
            return;
        }

        cacheStorage.Dispose();
        cacheStorage = null;
    }
}