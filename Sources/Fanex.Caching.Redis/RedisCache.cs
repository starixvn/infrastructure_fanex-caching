namespace Fanex.Caching.Redis
{
    using System;
    using System.Threading.Tasks;
    using StackExchange.Redis;

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

            try
            {
                return RedisStorage.TryGet(key, out value);
            }
            catch (RedisTimeoutException e)
            {
                throw new Exception($"RedisTimeoutException on key: {key}", e);
            }

        }

        public Task<(bool found, T value)> TryGetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            try
            {
                return RedisStorage.TryGetAsync<T>(key);
            }
            catch (RedisTimeoutException e)
            {
                throw new Exception($"RedisTimeoutException on key: {key}", e);
            }

        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            VerifyParametersOfSet(key, value, options);

            RedisStorage.Set(key, value, options);
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            VerifyParametersOfSet(key, value, options);

            return RedisStorage.SetAsync(key, value, options);
        }

        private static void VerifyParametersOfSet<T>(string key, T value, CacheItemOptions options)
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