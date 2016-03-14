namespace Fanex.Caching.MemoryCache
{
    public enum EvictionReason
    {
        None,
        Removed,
        Replaced,
        Expired,
        TokenExpired,
        Capacity
    }
}