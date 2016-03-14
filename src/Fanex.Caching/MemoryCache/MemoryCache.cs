namespace Fanex.Caching.MemoryCache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MemoryCache : Cache
    {
        private static readonly Task CompletedTask = Task.FromResult<object>(null);

        private readonly MemoryStorage _memoryStorage;

        public MemoryCache()
            : this(new MemoryCacheOptions())
        {
        }

        public MemoryCache(MemoryCacheOptions options)
        {
            options = options ?? new MemoryCacheOptions();
            _memoryStorage = new MemoryStorageImpl(options);
        }

        public MemoryCache(Hashtable settings)
        {
            var options = ObjectMapper.ConvertTo<MemoryCacheOptions>(
                settings,
                new Dictionary<string, Func<object, object>>()
                {
                    {
                        nameof(MemoryCacheOptions.ExpirationScanFrequency),
                        (object expirationScanFrequency) => TimeSpan.FromSeconds(Convert.ToDouble(expirationScanFrequency))
                    }
                });

            _memoryStorage = new MemoryStorageImpl(options);
        }

        public bool Contain(string key)
        {
            bool contained = false;

            if (!string.IsNullOrEmpty(key))
            {
                contained = _memoryStorage.Contain(key);
            }

            return contained;
        }

        public Task<bool> ContainAsync(string key)
        {
            return Task.FromResult(Contain(key));
        }

        public T Get<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return (T)_memoryStorage.Get(key);
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Task.FromResult(Get<T>(key));
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _memoryStorage.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);

            return CompletedTask;
        }

        public void Set<T>(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var memoryCacheItemOptions = new MemoryCacheItemOptions
            {
                RelativeExpiration = CacheItemOptions.DefaultRelativeExpiration
            };

            _memoryStorage.Set(key, value, memoryCacheItemOptions);
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var memoryCacheItemOptions = new MemoryCacheItemOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                RelativeExpiration = options.RelativeExpiration,
                SlidingExpiration = options.SlidingExpiration
            };

            _memoryStorage.Set(key, value, memoryCacheItemOptions);
        }

        public void Set<T>(string key, T value, MemoryCacheItemOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _memoryStorage.Set(key, value, options);
        }

        public Task SetAsync<T>(string key, T value)
        {
            Set(key, value);

            return CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            Set(key, value, options);

            return CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, MemoryCacheItemOptions options)
        {
            Set(key, value, options);

            return CompletedTask;
        }
    }
}