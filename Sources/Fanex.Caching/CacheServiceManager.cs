namespace Fanex.Caching
{
    using System;
    using Fanex.Caching.Memory;

    public static class CacheServiceManager
    {
        private static ICache _cacheStorage;
        private static Func<ICache> _cacheStorageFactory;

        internal static ICache CacheStorage
        {
            get
            {
                if (_cacheStorage == null)
                {
                    _cacheStorage = _cacheStorageFactory != null ? _cacheStorageFactory() : new MemoryCache();
                }

                return _cacheStorage;
            }
        }

        public static void Use(ICache cacheStorage)
        {
            if (cacheStorage == null)
            {
                throw new ArgumentNullException(nameof(cacheStorage));
            }

            if (_cacheStorage != null)
            {
                _cacheStorage.Dispose();
            }

            _cacheStorage = cacheStorage;
        }

        public static void Use(Func<ICache> cacheStorageFactory)
            => _cacheStorageFactory = cacheStorageFactory ?? throw new ArgumentNullException(nameof(cacheStorageFactory));

        internal static void Reset()
        {
            _cacheStorage = null;
            _cacheStorageFactory = null;
        }
    }
}