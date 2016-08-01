namespace Fanex.Caching
{
    using System;
    using System.Threading.Tasks;

    public static class CacheService
    {
        private static Cache _cacheStorage;

        public static Cache CacheStorage
        {
            get
            {
                if (_cacheStorage == null)
                {
                    _cacheStorage = CacheServiceManager.CacheStorage;
                }

                return _cacheStorage;
            }
        }

        public static Cache SwitchCacheStorage(Cache newCacheStorage)
        {
            if (newCacheStorage == null)
            {
                throw new ArgumentNullException(nameof(newCacheStorage));
            }

            Cache oldCacheStorage = _cacheStorage;

            _cacheStorage = newCacheStorage;

            return oldCacheStorage;
        }

        public static bool Contain(string key)
            => CacheStorage.Contain(key);

        public static Task<bool> ContainAsync(string key)
            => CacheStorage.ContainAsync(key);

        public static T Get<T>(string key)
        {
            T value = default(T);

            CacheStorage.TryGet(key, out value);

            return value;
        }

        public static async Task<T> GetAsync<T>(string key)
        {
            Tuple<bool, T> result = await CacheStorage.TryGetAsync<T>(key);

            return result.Item2;
        }

        public static bool TryGet<T>(string key, out T value)
            => CacheStorage.TryGet(key, out value);

        public static Task<Tuple<bool, T>> TryGetAsync<T>(string key)
            => CacheStorage.TryGetAsync<T>(key);

        public static T GetOrSet<T>(string key, Func<T> factory)
        {
            T value = default(T);
            bool found = CacheStorage.TryGet(key, out value);

            if (!found && factory != null)
            {
                var cacheItemOptions = new CacheItemOptions
                {
                    RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
                };

                value = factory();
                CacheStorage.Set(key, value, cacheItemOptions);
            }

            return value;
        }

        public static T GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            T value = default(T);
            bool found = CacheStorage.TryGet(key, out value);

            if (!found && factory != null)
            {
                value = factory();
                CacheStorage.Set(key, value, options);
            }

            return value;
        }

        public static async Task<T> GetOrSetAsync<T>(string key, Func<T> factory)
        {
            T value = default(T);
            Tuple<bool, T> found = await CacheStorage.TryGetAsync<T>(key);

            if (!found.Item1 && factory != null)
            {
                var cacheItemOptions = new CacheItemOptions
                {
                    RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
                };

                value = factory();
                await CacheStorage.SetAsync(key, value, cacheItemOptions);
            }
            else
            {
                value = found.Item2;
            }

            return value;
        }

        public static async Task<T> GetOrSetAsync<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            T value = default(T);
            Tuple<bool, T> found = await CacheStorage.TryGetAsync<T>(key);

            if (!found.Item1 && factory != null)
            {
                value = factory();
                await CacheStorage.SetAsync(key, value, options);
            }
            else
            {
                value = found.Item2;
            }

            return value;
        }

        public static void Remove(string key)
            => CacheStorage.Remove(key);

        public static Task RemoveAsync(string key)
            => CacheStorage.RemoveAsync(key);

        public static void Set<T>(string key, T value)
        {
            var cacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            CacheStorage.Set(key, value, cacheItemOptions);
        }

        public static void Set<T>(string key, T value, CacheItemOptions options)
            => CacheStorage.Set(key, value, options);

        public static Task SetAsync<T>(string key, T value)
        {
            var cacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            return CacheStorage.SetAsync(key, value, cacheItemOptions);
        }

        public static Task SetAsync<T>(string key, T value, CacheItemOptions options)
            => CacheStorage.SetAsync(key, value, options);
    }
}