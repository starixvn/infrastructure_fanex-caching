namespace Fanex.Caching
{
    public abstract class CacheItem
    {
        protected CacheItem(object key, object value, CacheItemOptions options)
        {
            Key = key;
            Value = value;
            Options = options;
        }

        public object Key { get; }

        public object Value { get; }

        public CacheItemOptions Options { get; }
    }
}