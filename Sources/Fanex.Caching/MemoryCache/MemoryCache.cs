namespace Fanex.Caching.Memory
{
    using System;
    using System.Threading.Tasks;
    using static System.Threading.Tasks.Task;

#if NET461

    using System.Collections;
    using System.Collections.Generic;
    using Fanex.Caching.Configuration;

#else

    using Microsoft.Extensions.Options;

#endif

    public class MemoryCache : Cache
    {
        private readonly MemoryStorage _memoryStorage;

        public MemoryCache()
            : this(new MemoryCacheOptions())
        {
        }

#if NET461

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

#else
        public MemoryCache(IOptions<MemoryCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _memoryStorage = new MemoryStorageImpl(options.Value, new SystemClockImpl());
        }
#endif

        public bool Contain(string key)
            => !string.IsNullOrEmpty(key) && _memoryStorage.Contain(key);

        public Task<bool> ContainAsync(string key)
            => FromResult(Contain(key));

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            bool found = _memoryStorage.TryGet(key, out object result);

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
            bool found = TryGet(key, out T value);

            return FromResult(new Tuple<bool, T>(found, value));
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

            if (ReferenceEquals(value, null))
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

            if (ReferenceEquals(value, null))
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