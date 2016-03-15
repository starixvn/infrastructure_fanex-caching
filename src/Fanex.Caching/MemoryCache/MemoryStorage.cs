namespace Fanex.Caching.Memory
{
    internal interface MemoryStorage
    {
        bool Contain(object key);

        object Get(object key);

        void Set(object key, object value, MemoryCacheItemOptions options);

        void Remove(object key);
    }
}