namespace Fanex.Caching.Redis
{
    using System;
    using System.Threading.Tasks;

    public sealed class RedisCache : ICache
    {
        public RedisCache(RedisCacheOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            RedisStorage = new RedisStorage(options);
        }

        internal RedisStorage RedisStorage { get; private set; }

        public bool Contain(string key)
            => !string.IsNullOrEmpty(key) && RedisStorage.Contain(key);

        public Task<bool> ContainAsync(string key)
            => string.IsNullOrEmpty(key) ? Task.FromResult(false) : RedisStorage.ContainAsync(key);

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            return RedisStorage.TryGet(key, out value);
        }

        public Task<Tuple<bool, T>> TryGetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            return RedisStorage.TryGetAsync<T>(key);
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            RedisStorage.Set<T>(key, value, options);
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return RedisStorage.SetAsync<T>(key, value, options);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            RedisStorage.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            return RedisStorage.RemoveAsync(key);
        }

        public void Dispose()
        {
            RedisStorage.Dispose();
            RedisStorage = null;
        }
    }
}