namespace Fanex.Caching.MemoryCache
{
    public delegate void PostEvictionDelegate(object key, object value, EvictionReason reason, object state);
}