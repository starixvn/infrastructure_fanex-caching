namespace Fanex.Caching
{
    public abstract class CacheItem
    {
        public CacheItem(object key, object value, CacheItemOptions options)
        {
            Key = key;
            Value = value;
            Options = options;
        }

        public object Key { get; private set; }

        public object Value { get; private set; }

        public CacheItemOptions Options { get; private set; }
    }
}