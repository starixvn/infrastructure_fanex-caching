namespace Fanex.Caching
{
    public class CacheItem
    {
        protected CacheItem(string key, object value, CacheItemOptions options)
        {
            Key = key;
            Value = value;
            Options = options;
        }

        public string Key { get; }

        public object Value { get; }

        public CacheItemOptions Options { get; }
    }
}