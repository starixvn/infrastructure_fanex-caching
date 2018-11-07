namespace Fanex.Caching
{
    using System;
    using System.Threading.Tasks;

    public interface ICache : IDisposable
    {
        bool Contain(string key);

        Task<bool> ContainAsync(string key);

        bool TryGet<T>(string key, out T value);

        Task<(bool, T)> TryGetAsync<T>(string key);

        void Set<T>(string key, T value, CacheItemOptions options);

        Task SetAsync<T>(string key, T value, CacheItemOptions options);

        void Remove(string key);

        Task RemoveAsync(string key);
    }
}