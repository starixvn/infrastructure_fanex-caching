namespace Fanex.Caching
{
    using System;
    using System.Threading.Tasks;

    public static class ConfiguredCacheService
    {
        private static CacheService _currentInstance;

        public static Cache CacheStorage
            => CurrentInstance.CacheStorage;

        private static CacheService CurrentInstance
        {
            get
            {
                if (_currentInstance == null)
                {
                    _currentInstance = new CacheServiceImpl();
                }

                return _currentInstance;
            }
        }

        [Obsolete("This method will be removed soon. Use 'new CacheService(Cache cacheStorage)' instead.")]
        public static Cache SwitchCacheStorage(Cache newCacheStorage)
        {
            if (newCacheStorage == null)
            {
                throw new ArgumentNullException(nameof(newCacheStorage));
            }

            Cache oldCacheStorage = CurrentInstance.CacheStorage;

            _currentInstance = new CacheServiceImpl(newCacheStorage);

            return oldCacheStorage;
        }

        internal static void Initialize(CacheService newInstance)
        {
            if (newInstance == null)
            {
                throw new ArgumentNullException(nameof(newInstance));
            }

            _currentInstance = newInstance;
        }

        public static bool Contain(string key)
            => CurrentInstance.Contain(key);

        public static Task<bool> ContainAsync(string key)
            => CurrentInstance.ContainAsync(key);

        public static T Get<T>(string key)
            => CurrentInstance.Get<T>(key);

        public static Task<T> GetAsync<T>(string key)
            => CurrentInstance.GetAsync<T>(key);

        public static bool TryGet<T>(string key, out T value)
            => CurrentInstance.TryGet(key, out value);

        public static Task<Tuple<bool, T>> TryGetAsync<T>(string key)
            => CurrentInstance.TryGetAsync<T>(key);

        public static T GetOrSet<T>(string key, Func<T> factory)
            => CurrentInstance.GetOrSet(key, factory);

        public static T GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options)
            => CurrentInstance.GetOrSet(key, factory, options);

        public static Task<T> GetOrSetAsync<T>(string key, Func<T> factory)
            => CurrentInstance.GetOrSetAsync(key, factory);

        public static Task<T> GetOrSetAsync<T>(string key, Func<T> factory, CacheItemOptions options)
            => CurrentInstance.GetOrSetAsync(key, factory, options);

        public static void Remove(string key)
            => CurrentInstance.Remove(key);

        public static Task RemoveAsync(string key)
            => CurrentInstance.RemoveAsync(key);

        public static void Set<T>(string key, T value)
            => CurrentInstance.Set(key, value);

        public static void Set<T>(string key, T value, CacheItemOptions options)
            => CurrentInstance.Set(key, value, options);

        public static Task SetAsync<T>(string key, T value)
            => CurrentInstance.SetAsync(key, value);

        public static Task SetAsync<T>(string key, T value, CacheItemOptions options)
            => CurrentInstance.SetAsync(key, value, options);
    }
}