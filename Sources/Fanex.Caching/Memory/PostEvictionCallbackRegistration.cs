namespace Fanex.Caching.Memory
{
    public delegate void PostEvictionDelegate(string key, object value, EvictionReason reason, object state);

    internal class PostEvictionCallbackRegistration
    {
        public PostEvictionDelegate EvictionCallback { get; set; }

        public object State { get; set; }
    }
}