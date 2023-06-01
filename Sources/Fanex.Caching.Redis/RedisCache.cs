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

        public async Task<bool> ContainAsync(string key)
            => string.IsNullOrEmpty(key) ? await Task.FromResult(false) : await RedisStorage.ContainAsync(key);

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
                throw new TimeoutException($"TryGet timeout on key: {key}", e);
            }

        }

        public async Task<(bool found, T value)> TryGetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            try
            {
                return await RedisStorage.TryGetAsync<T>(key);
            }
            catch (RedisTimeoutException e)
            {
                throw new TimeoutException($"TryGetAsync timeout on key: {key}. Inner exception: " + e.Message, e);
            }

        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            VerifyParametersOfSet(key, value, options);

            try
            {
                RedisStorage.Set(key, value, options);
            }
            catch (RedisTimeoutException e)
            {
                throw new TimeoutException($"RedisStorage.Set timeout on key: {key}. Inner exception: " + e.Message, e);
            }
        }

        public async Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            VerifyParametersOfSet(key, value, options);

            try
            {
                await RedisStorage.SetAsync(key, value, options);
            }
            catch (RedisTimeoutException e)
            {
                throw new TimeoutException($"RedisStorage.Set timeout on key: {key}. Inner exception: " + e.Message, e);
            }
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

            try
            {
                RedisStorage.Remove(key);
            }
            catch (RedisTimeoutException e)
            {
                throw new TimeoutException($"RedisStorage.Set timeout on key: {key}. Inner exception: " + e.Message, e);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            try
            {
                await RedisStorage.RemoveAsync(key);
            }
            catch (RedisTimeoutException e)
            {
                throw new TimeoutException($"RedisStorage.Set timeout on key: {key}. Inner exception: " + e.Message, e);
            }
        }

        public void Dispose()
        {
            RedisStorage.Dispose();
            RedisStorage = null;
        }
    }
}