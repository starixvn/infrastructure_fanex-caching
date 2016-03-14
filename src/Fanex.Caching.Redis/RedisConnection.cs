namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    internal class RedisConnection
    {
        private ConnectionMultiplexer _redisMultiplexer;
        private IDatabase _connection;
        private Serializer _serializer;
        private string _applicationName;

        public RedisConnection(RedisCacheOptions options)
        {
            ConfigurationOptions redisConnectionOptions = options.ToRedisConnectionOptions();
            int databaseId = options.DatabaseId;

            _applicationName = options.ApplicationName;
            _serializer = options.Serializer;
            _redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionOptions, null);
            _connection = _redisMultiplexer.GetDatabase(databaseId, null);
        }

        private string ModifyKey(string key)
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                return key;
            }
            else
            {
                return $"{_applicationName}:{key}";
            }
        }
    }
}