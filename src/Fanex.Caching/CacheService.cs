namespace Fanex.Caching
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using Configuration;
    using Memory;

    public static class CacheService
    {
        private static Cache _cacheStorage;

        static CacheService()
        {
            CacheSettings settings = CacheSettings.GetSettings();

            if (settings != null)
            {
                Type cacheType = string.IsNullOrEmpty(settings.CacheType)
                    ? typeof(MemoryCache)
                    : Type.GetType(settings.CacheType);
                Hashtable connectionSettings = settings?.ConnectionSettingCollection?.ToHashtable();

                if (connectionSettings != null)
                {
                    _cacheStorage = Activator.CreateInstance(cacheType, connectionSettings) as Cache;
                }
                else
                {
                    _cacheStorage = Activator.CreateInstance(cacheType) as Cache;
                }
            }
            else
            {
                _cacheStorage = new MemoryCache();
            }
        }

        public static Cache CacheStorage
        {
            get
            {
                return _cacheStorage;
            }
        }

        public static bool Contain(string key)
        {
            return _cacheStorage.Contain(key);
        }

        public static Task<bool> ContainAsync(string key)
        {
            return _cacheStorage.ContainAsync(key);
        }

        public static T Get<T>(string key)
        {
            T value = default(T);

            _cacheStorage.TryGet(key, out value);

            return value;
        }

        public static async Task<T> GetAsync<T>(string key)
        {
            Tuple<bool, T> result = await _cacheStorage.TryGetAsync<T>(key);

            return result.Item2;
        }

        public static bool TryGet<T>(string key, out T value)
        {
            return _cacheStorage.TryGet(key, out value);
        }

        public static Task<Tuple<bool, T>> TryGetAsync<T>(string key)
        {
            return _cacheStorage.TryGetAsync<T>(key);
        }

        public static T GetOrSet<T>(string key, Func<T> factory)
        {
            T value = default(T);
            bool found = _cacheStorage.TryGet(key, out value);

            if (!found && factory != null)
            {
                var cacheItemOptions = new CacheItemOptions
                {
                    RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
                };

                value = factory();
                _cacheStorage.Set(key, value, cacheItemOptions);
            }

            return value;
        }

        public static T GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            T value = default(T);
            bool found = _cacheStorage.TryGet(key, out value);

            if (!found && factory != null)
            {
                value = factory();
                _cacheStorage.Set(key, value, options);
            }

            return value;
        }

        public static async Task<T> GetOrSetAsync<T>(string key, Func<T> factory)
        {
            T value = default(T);
            Tuple<bool, T> found = await _cacheStorage.TryGetAsync<T>(key);

            if (!found.Item1 && factory != null)
            {
                var cacheItemOptions = new CacheItemOptions
                {
                    RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
                };

                value = factory();
                await _cacheStorage.SetAsync(key, value, cacheItemOptions);
            }

            return value;
        }

        public static async Task<T> GetOrSetAsync<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            T value = default(T);
            Tuple<bool, T> found = await _cacheStorage.TryGetAsync<T>(key);

            if (!found.Item1 && factory != null)
            {
                value = factory();
                await _cacheStorage.SetAsync(key, value, options);
            }

            return value;
        }

        public static void Remove(string key)
        {
            _cacheStorage.Remove(key);
        }

        public static Task RemoveAsync(string key)
        {
            return _cacheStorage.RemoveAsync(key);
        }

        public static void Set<T>(string key, T value)
        {
            var cacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            _cacheStorage.Set(key, value, cacheItemOptions);
        }

        public static void Set<T>(string key, T value, CacheItemOptions options)
        {
            _cacheStorage.Set(key, value, options);
        }

        public static Task SetAsync<T>(string key, T value)
        {
            var cacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            return _cacheStorage.SetAsync(key, value, cacheItemOptions);
        }

        public static Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            return _cacheStorage.SetAsync(key, value, options);
        }
    }
}