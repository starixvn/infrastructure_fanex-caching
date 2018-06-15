namespace Fanex.Caching.Redis
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    internal sealed class RedisStorage : IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private readonly ConfigurationOptions _redisConnectionOptions;
        private readonly ISerializer _serializer;
        private readonly string _applicationName;
        private volatile ConnectionMultiplexer _connection;
        private IDatabase _storage;

        public RedisStorage(RedisCacheOptions options)
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
            await ConnectAsync().ConfigureAwait(false);

            return await _storage.KeyExistsAsync(ModifyKey(key)).ConfigureAwait(false);
        }

        public bool TryGet<T>(string key, out T value)
        {
            Connect();

            var item = new RedisCacheItem(ModifyKey(key), RedisValue.Null, null);
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
            await ConnectAsync().ConfigureAwait(false);

            var item = new RedisCacheItem(ModifyKey(key), RedisValue.Null, null);
            RedisCommand command = item.ToGetCommand();
            RedisResult dataFromRedis = await _storage.ScriptEvaluateAsync(command.Script, command.Keys).ConfigureAwait(false);
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
            await ConnectAsync().ConfigureAwait(false);
            await _storage.KeyDeleteAsync(ModifyKey(key)).ConfigureAwait(false);
        }

        public void Set<T>(string key, T value, CacheItemOptions options)
        {
            var item = new RedisCacheItem(ModifyKey(key), value, options);
            RedisCommand command = item.ToSetCommand();

            Connect();
            _storage.ScriptEvaluate(command.Script, command.Keys, command.Values);
        }

        public async Task SetAsync<T>(string key, T value, CacheItemOptions options)
        {
            var item = new RedisCacheItem(ModifyKey(key), value, options);
            RedisCommand command = item.ToSetCommand();

            await ConnectAsync().ConfigureAwait(false);
            await _storage.ScriptEvaluateAsync(command.Script, command.Keys, command.Values).ConfigureAwait(false);
        }

#pragma warning disable S2589 // Boolean expressions should not be gratuitous

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

            await _connectionLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_connection == null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionOptions).ConfigureAwait(false);
                    _storage = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

#pragma warning restore S2589 // Boolean expressions should not be gratuitous

        private string ModifyKey(string key)
            => string.IsNullOrEmpty(_applicationName) ? key : $"{_applicationName}:{key}";

        public void Dispose()
        {
            _connectionLock.Dispose();
            _connection.Dispose();
            _connection = null;
        }
    }
}