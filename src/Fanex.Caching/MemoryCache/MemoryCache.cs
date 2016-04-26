namespace Fanex.Caching.Memory
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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _memoryStorage = new MemoryStorageImpl(options, new SystemClockImpl());
        }

        public MemoryCache(Hashtable settings)
        {
            var options = ObjectMapper.ConvertTo<MemoryCacheOptions>(
                settings,
                new Dictionary<string, Func<object, object>>()
                {
                    {
                        nameof(MemoryCacheOptions.ExpirationScanFrequency),
                        expirationScanFrequency => TimeSpan.FromSeconds(Convert.ToDouble(expirationScanFrequency))
                    }
                });

            _memoryStorage = new MemoryStorageImpl(options, new SystemClockImpl());
        }

        public bool Contain(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return _memoryStorage.Contain(key);
            }

            return false;
        }

        public Task<bool> ContainAsync(string key)
        {
            return Task.FromResult(Contain(key));
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            object result = null;
            bool found = _memoryStorage.TryGet(key, out result);

            if ((result != null) && (result is T))
            {
                value = (T)result;
            }
            else
            {
                value = default(T);
                found = false;
            }

            return found;
        }

        public Task<Tuple<bool, T>> TryGetAsync<T>(string key)
        {
            T value = default(T);
            bool found = TryGet(key, out value);

            return Task.FromResult(new Tuple<bool, T>(found, value));
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
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

            var memoryCacheItemOptions = options as MemoryCacheItemOptions;

            memoryCacheItemOptions = memoryCacheItemOptions ?? new MemoryCacheItemOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                RelativeExpiration = options.RelativeExpiration,
                SlidingExpiration = options.SlidingExpiration
            };

            _memoryStorage.Set(key, value, memoryCacheItemOptions);
        }

        public void Set<T>(string key, T value, MemoryCacheItemOptions options)
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

            _memoryStorage.Set(key, value, options);
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