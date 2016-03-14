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
            return _cacheStorage.Get<T>(key);
        }

        public static Task<T> GetAsync<T>(string key)
        {
            return _cacheStorage.GetAsync<T>(key);
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
            _cacheStorage.Set<T>(key, value);
        }

        public static void Set<T>(string key, T value, CacheItemOptions options)
        {
            _cacheStorage.Set<T>(key, value, options);
        }

        public static Task SetAsync<T>(string key, T value)
        {
            return _cacheStorage.SetAsync<T>(key, value);
        }

        public static Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            return _cacheStorage.SetAsync<T>(key, value, options);
        }
    }
}