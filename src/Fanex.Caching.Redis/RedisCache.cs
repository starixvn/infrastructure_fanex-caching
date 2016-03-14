namespace Fanex.Caching.Redis
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RedisCache : Cache
    {
        private readonly RedisConnection _redisConnection;

        public RedisCache()
            : this(new RedisCacheOptions())
        {
        }

        public RedisCache(RedisCacheOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _redisConnection = new RedisConnection(options);
        }

        public RedisCache(Hashtable settings)
        {
            var options = ObjectMapper.ConvertTo<RedisCacheOptions>(
                settings,
                new Dictionary<string, Func<object, object>>()
                {
                    {
                        nameof(RedisCacheOptions.Serializer),
                        (object serializerTypeName) =>
                        {
                            Type serializerType = Type.GetType((string)serializerTypeName);

                            if (serializerType != null)
                            {
                                return Activator.CreateInstance(serializerType);
                            }

                            throw new ArgumentException("Invalid serializer type.");
                        }
                    }
                });

            _redisConnection = new RedisConnection(options);
        }

        public bool Contain(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return _redisConnection.Contain(key);
            }

            return false;
        }

        public Task<bool> ContainAsync(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return _redisConnection.ContainAsync(key);
            }

            return Task.FromResult(false);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _redisConnection.Get<T>(key);
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _redisConnection.GetAsync<T>(key);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            _redisConnection.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _redisConnection.RemoveAsync(key);
        }

        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var redisCacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            _redisConnection.Set(key, value, redisCacheItemOptions);
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _redisConnection.Set(key, value, options);
        }

        public Task SetAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var redisCacheItemOptions = new CacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            return _redisConnection.SetAsync(key, value, redisCacheItemOptions);
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return _redisConnection.SetAsync(key, value, options);
        }
    }
}