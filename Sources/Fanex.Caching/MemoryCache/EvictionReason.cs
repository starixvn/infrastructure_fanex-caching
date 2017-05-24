namespace Fanex.Caching.Memory
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