namespace Fanex.Caching.Redis
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    internal class RedisConnection
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private volatile ConnectionMultiplexer _connection;
        private IDatabase _storage;
        private ConfigurationOptions _redisConnectionOptions;
        private Serializer _serializer;
        private string _applicationName;

        public RedisConnection(RedisCacheOptions options)
        {
            _redisConnectionOptions = options.ToRedisConnectionOptions();
            _serializer = options.Serializer;
            _applicationName = options.ApplicationName;
        }

        public bool Contain(string key)
        {
            Connect();

            return _storage.KeyExists(ModifyKey(key));
        }

        public async Task<bool> ContainAsync(string key)
        {
            await ConnectAsync();

            return await _storage.KeyExistsAsync(ModifyKey(key));
        }

        public bool TryGet<T>(string key, out T value)
        {
            Connect();

            var item = new RedisCacheItem(ModifyKey(key), null, null, _serializer);
            RedisCommand command = item.ToGetCommand();
            RedisResult dataFromRedis = _storage.ScriptEvaluate(command.Script, command.Keys);
            bool found = false;

            value = default(T);

            if (!dataFromRedis.IsNull)
            {
                value = (T)RedisValueHelper.ConvertToObject((RedisValue)dataFromRedis, typeof(T), _serializer);
                found = true;
            }

            return found;
        }

        public async Task<Tuple<bool, T>> TryGetAsync<T>(string key)
        {
            await ConnectAsync();

            var item = new RedisCacheItem(ModifyKey(key), null, null, _serializer);
            RedisCommand command = item.ToGetCommand();
            RedisResult dataFromRedis = await _storage.ScriptEvaluateAsync(command.Script, command.Keys);
            bool found = false;
            T value = default(T);

            if (!dataFromRedis.IsNull)
            {
                value = (T)RedisValueHelper.ConvertToObject((RedisValue)dataFromRedis, typeof(T), _serializer);
                found = true;
            }

            return new Tuple<bool, T>(found, value);
        }

        public void Remove(string key)
        {
            Connect();
            _storage.KeyDelete(ModifyKey(key));
        }

        public async Task RemoveAsync(string key)
        {
            await ConnectAsync();
            await _storage.KeyDeleteAsync(ModifyKey(key));
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            var item = new RedisCacheItem(ModifyKey(key), value, options, _serializer);
            RedisCommand command = item.ToSetCommand();

            Connect();
            _storage.ScriptEvaluate(command.Script, command.Keys, command.Values);
        }

        public async Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            var item = new RedisCacheItem(ModifyKey(key), value, options, _serializer);
            RedisCommand command = item.ToSetCommand();

            await ConnectAsync();
            await _storage.ScriptEvaluateAsync(command.Script, command.Keys, command.Values);
        }

        private void Connect()
        {
            if (_connection != null)
            {
                return;
            }

            _connectionLock.Wait();

            try
            {
                if (_connection == null)
                {
                    _connection = ConnectionMultiplexer.Connect(_redisConnectionOptions);
                    _storage = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync()
        {
            if (_connection != null)
            {
                return;
            }

#if !NET40
            await _connectionLock.WaitAsync();
#else
            _connectionLock.Wait();
#endif

            try
            {
                if (_connection == null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionOptions);
                    _storage = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
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