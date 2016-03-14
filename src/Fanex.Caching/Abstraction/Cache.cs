namespace Fanex.Caching
{
    using System.Threading.Tasks;

    public interface Cache
    {
        bool Contain(string key);

        Task<bool> ContainAsync(string key);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);

        void Set<T>(string key, T value);

        void Set<T>(string key, T value, CacheItemOptions options);

        Task SetAsync<T>(string key, T value);

        Task SetAsync<T>(string key, T value, CacheItemOptions options);

        void Remove(string key);

        Task RemoveAsync(string key);
    }
}