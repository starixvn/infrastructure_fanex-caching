namespace Fanex.Caching.Memory
{
    internal interface MemoryStorage
    {
        bool Contain(object key);

        bool TryGet(object key, out object value);

        void Set(object key, object value, MemoryCacheItemOptions options);

        void Remove(object key);
    }
}