namespace Fanex.Caching.Memory
{
    using System;
    using System.Threading.Tasks;

    public sealed class MemoryCache : ICache
    {
        public MemoryCache(MemoryCacheOptions options = null)
            => MemoryStorage = new MemoryStorage(options ?? new MemoryCacheOptions());

        internal IMemoryStorage MemoryStorage { get; private set; }

        public bool Contain(string key)
            => !string.IsNullOrEmpty(key) && MemoryStorage.Contain(key);

        public Task<bool> ContainAsync(string key)
            => Task.FromResult(Contain(key));

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            bool found = MemoryStorage.TryGet(key, out object result);

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

            return Task.FromResult(new Tuple<bool, T>(found, value));
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
            => SetInternal(key, value, options);

        public void Set<T>(string key, T value, MemoryCacheItemOptions options)
            => SetInternal(key, value, options);

        private void SetInternal<T>(string key, T value, CacheItemOptions options)
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

            var memoryCacheItemOptions = options as MemoryCacheItemOptions;

            memoryCacheItemOptions = memoryCacheItemOptions ?? new MemoryCacheItemOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                RelativeExpiration = options.RelativeExpiration,
                SlidingExpiration = options.SlidingExpiration
            };

            MemoryStorage.Set(key, value, memoryCacheItemOptions);
        }

        public Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            Set(key, value, options);

            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, MemoryCacheItemOptions options)
        {
            Set(key, value, options);

            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must not be null or empty.", nameof(key));
            }

            MemoryStorage.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            MemoryStorage.Dispose();
            MemoryStorage = null;
        }
    }
}