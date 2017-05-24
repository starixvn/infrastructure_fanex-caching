namespace Fanex.Caching.Memory
{
    public delegate void PostEvictionDelegate(object key, object value, EvictionReason reason, object state);
}