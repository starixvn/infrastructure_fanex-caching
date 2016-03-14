namespace Fanex.Caching.Redis
{
    using System;
    using System.Threading.Tasks;

    public class RedisCache : Cache
    {
        private readonly RedisConnection _redisConnection;

        public RedisCache(RedisCacheOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _redisConnection = new RedisConnection(options);
        }

        public bool Contain(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainAsync(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public void Touch(string key)
        {
            throw new NotImplementedException();
        }

        public Task TouchAsync(string key)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            throw new NotImplementedException();
        }
    }
}