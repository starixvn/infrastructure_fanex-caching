namespace Fanex.Caching.Redis
{
    using System.Threading.Tasks;
    using StackExchange.Redis;

    internal class RedisConnection
    {
        private ConnectionMultiplexer _redisMultiplexer;
        private IDatabase _connection;
        private ConfigurationOptions _redisConnectionOptions;
        private int _databaseId;
        private Serializer _serializer;
        private string _applicationName;

        public RedisConnection(RedisCacheOptions options)
        {
            _redisConnectionOptions = options.ToRedisConnectionOptions();
            _databaseId = options.DatabaseId;
            _serializer = options.Serializer;
            _applicationName = options.ApplicationName;
        }

        public bool Contain(string key)
        {
            Connect();

            return _connection.KeyExists(ModifyKey(key));
        }

        public async Task<bool> ContainAsync(string key)
        {
            await ConnectAsync();

            return await _connection.KeyExistsAsync(ModifyKey(key));
        }

        public T Get<T>(string key)
        {
            Connect();

            T value = default(T);
            RedisCacheItem item = new RedisCacheItem(ModifyKey(key), null, null, _serializer);
            RedisCommand command = item.GetGetCommand();
            RedisResult dataFromRedis = _connection.ScriptEvaluate(command.Script, command.Keys);

            if (dataFromRedis != null)
            {
                value = (T)RedisValueHelper.ConvertToObject((RedisValue)dataFromRedis, typeof(T), _serializer);
            }

            return value;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            await ConnectAsync();

            T value = default(T);
            RedisCacheItem item = new RedisCacheItem(ModifyKey(key), null, null, _serializer);
            RedisCommand command = item.GetGetCommand();
            RedisResult dataFromRedis = await _connection.ScriptEvaluateAsync(command.Script, command.Keys);

            if (dataFromRedis != null)
            {
                value = (T)RedisValueHelper.ConvertToObject((RedisValue)dataFromRedis, typeof(T), _serializer);
            }

            return value;
        }

        public void Remove(string key)
        {
            Connect();
            _connection.KeyDelete(ModifyKey(key));
        }

        public async Task RemoveAsync(string key)
        {
            await ConnectAsync();
            await _connection.KeyDeleteAsync(ModifyKey(key));
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            RedisCacheItem item = new RedisCacheItem(ModifyKey(key), value, options, _serializer);
            RedisCommand command = item.GetSetCommand();

            Connect();
            _connection.ScriptEvaluate(command.Script, command.Keys, command.Values);
        }

        public async Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            RedisCacheItem item = new RedisCacheItem(ModifyKey(key), value, options, _serializer);
            RedisCommand command = item.GetSetCommand();

            await ConnectAsync();
            await _connection.ScriptEvaluateAsync(command.Script, command.Keys, command.Values);
        }

        private void Connect()
        {
            if (_redisMultiplexer == null)
            {
                _redisMultiplexer = ConnectionMultiplexer.Connect(_redisConnectionOptions);
                _connection = _redisMultiplexer.GetDatabase(_databaseId);
            }
        }

        private async Task ConnectAsync()
        {
            if (_redisMultiplexer == null)
            {
                _redisMultiplexer = await ConnectionMultiplexer.ConnectAsync(_redisConnectionOptions);
                _connection = _redisMultiplexer.GetDatabase(_databaseId);
            }
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