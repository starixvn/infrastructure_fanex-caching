namespace Fanex.Caching
{
    using System;
    using System.Threading.Tasks;

    public class CacheServiceImpl : CacheService
    {
        private readonly Cache _cacheStorage;

        public CacheServiceImpl()
            => _cacheStorage = CacheServiceManager.CacheStorage;

        public CacheServiceImpl(Cache cacheStorage)
            => _cacheStorage = cacheStorage ?? throw new ArgumentNullException(nameof(cacheStorage));

        public Cache CacheStorage => _cacheStorage;

        public bool Contain(string key)
            => CacheStorage.Contain(key);

        public Task<bool> ContainAsync(string key)
            => CacheStorage.ContainAsync(key);

        public T Get<T>(string key)
        {
            CacheStorage.TryGet(key, out T value);

            return value;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            Tuple<bool, T> result = await CacheStorage.TryGetAsync<T>(key);

            return result != null ? result.Item2 : default(T);
        }

        public bool TryGet<T>(string key, out T value)
            => CacheStorage.TryGet(key, out value);

        public Task<Tuple<bool, T>> TryGetAsync<T>(string key)
            => CacheStorage.TryGetAsync<T>(key);

        public T GetOrSet<T>(string key, Func<T> factory)
        {
            bool found = CacheStorage.TryGet(key, out T value);

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

        public T GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            bool found = CacheStorage.TryGet(key, out T value);

            if (!found && factory != null)
            {
                value = factory();
                CacheStorage.Set(key, value, options);
            }

            return value;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<T> factory)
        {
            T value;
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

        public async Task<T> GetOrSetAsync<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            T value;
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

        public void Remove(string key)
            => CacheStorage.Remove(key);

        public Task RemoveAsync(string key)
            => CacheStorage.RemoveAsync(key);

        public void Set<T>(string key, T value)
        {
            var cacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            CacheStorage.Set(key, value, cacheItemOptions);
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
            => CacheStorage.Set(key, value, options);

        public Task SetAsync<T>(string key, T value)
        {
            var cacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            return CacheStorage.SetAsync(key, value, cacheItemOptions);
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
            => CacheStorage.SetAsync(key, value, options);
    }
}