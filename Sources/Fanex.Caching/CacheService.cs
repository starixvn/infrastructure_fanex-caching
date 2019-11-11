namespace Fanex.Caching
{
    using System;
    using System.Threading.Tasks;

#pragma warning disable S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)

    public class CacheService : ICacheService
    {
        public CacheService(ICache cacheStorage = null)
            => CacheStorage = cacheStorage ?? CacheServiceManager.CacheStorage;

        public ICache CacheStorage { get; }

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
            var (_, value) = await CacheStorage.TryGetAsync<T>(key).ConfigureAwait(false);

            return value;
        }

        public bool TryGet<T>(string key, out T value)
            => CacheStorage.TryGet(key, out value);

        public Task<(bool found, T value)> TryGetAsync<T>(string key)
            => CacheStorage.TryGetAsync<T>(key);

        public T GetOrSet<T>(string key, Func<T> factory)
        {
            var found = CacheStorage.TryGet(key, out T value);

            if (!found && factory != null)
            {
                var options = new CacheItemOptions().UseDefaultExpiration();

                value = factory();
                CacheStorage.Set(key, value, options);
            }

            return value;
        }

        public T GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options)
        {
            var found = CacheStorage.TryGet(key, out T value);

            if (!found && factory != null)
            {
                value = factory();
                CacheStorage.Set(key, value, options);
            }

            return value;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
        {
            var (found, value) = await CacheStorage.TryGetAsync<T>(key).ConfigureAwait(false);

            if (!found && factory != null)
            {
                var options = new CacheItemOptions().UseDefaultExpiration();

                value = await factory().ConfigureAwait(false);
                await CacheStorage.SetAsync(key, value, options).ConfigureAwait(false);
            }

            return value;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheItemOptions options)
        {
            var (found, value) = await CacheStorage.TryGetAsync<T>(key).ConfigureAwait(false);

            if (!found && factory != null)
            {
                value = await factory().ConfigureAwait(false);
                await CacheStorage.SetAsync(key, value, options).ConfigureAwait(false);
            }

            return value;
        }

        public void Remove(string key)
            => CacheStorage.Remove(key);

        public Task RemoveAsync(string key)
            => CacheStorage.RemoveAsync(key);

        public void Set<T>(string key, T value)
        {
            var options = new CacheItemOptions().UseDefaultExpiration();

            CacheStorage.Set(key, value, options);
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
            => CacheStorage.Set(key, value, options);

        public Task SetAsync<T>(string key, T value)
        {
            var options = new CacheItemOptions().UseDefaultExpiration();

            return CacheStorage.SetAsync(key, value, options);
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
            => CacheStorage.SetAsync(key, value, options);
    }
}