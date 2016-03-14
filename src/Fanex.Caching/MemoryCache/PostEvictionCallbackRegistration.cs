namespace Fanex.Caching.Memory
{
    internal class PostEvictionCallbackRegistration
    {
        public PostEvictionDelegate EvictionCallback { get; set; }

        public object State { get; set; }
    }
}