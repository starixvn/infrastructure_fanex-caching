namespace Fanex.Caching
{
    using System;
    using System.Threading.Tasks;

    public interface CacheService
    {
        Cache CacheStorage { get; }

        bool Contain(string key);

        Task<bool> ContainAsync(string key);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);

        bool TryGet<T>(string key, out T value);

        Task<Tuple<bool, T>> TryGetAsync<T>(string key);

        T GetOrSet<T>(string key, Func<T> factory);

        T GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options);

        Task<T> GetOrSetAsync<T>(string key, Func<T> factory);

        Task<T> GetOrSetAsync<T>(string key, Func<T> factory, CacheItemOptions options);

        void Remove(string key);

        Task RemoveAsync(string key);

        void Set<T>(string key, T value);

        void Set<T>(string key, T value, CacheItemOptions options);

        Task SetAsync<T>(string key, T value);

        Task SetAsync<T>(string key, T value, CacheItemOptions options);
    }
}