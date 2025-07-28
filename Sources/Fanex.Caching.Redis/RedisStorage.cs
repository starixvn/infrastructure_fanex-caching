namespace Fanex.Caching.Redis;

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

internal sealed partial class RedisStorage(RedisCacheOptions storageOptions) : IDisposable
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly ConfigurationOptions _redisConnectionOptions = storageOptions.ToRedisConnectionOptions();
    private readonly ISerializer _serializer = storageOptions.Serializer;
    private readonly ISystemClock _clock = storageOptions.Clock;
    private readonly string _applicationName = storageOptions.ApplicationName;

    private volatile ConnectionMultiplexer _connection;
    private IDatabase _database;

    public bool Contain(string key)
    {
        Connect();

        var redisKey = ModifyKey(key);
        var redisCommand = RedisCommand.CreateGetCommand(redisKey);
        var redisResult = EvaluateCommand(redisCommand);

        return !redisResult.IsNull;
    }

    public async Task<bool> ContainAsync(string key)
    {
        await ConnectAsync().ConfigureAwait(false);

        var redisKey = ModifyKey(key);
        var redisCommand = RedisCommand.CreateGetCommand(redisKey);
        var redisResult = await EvaluateCommandAsync(redisCommand).ConfigureAwait(false);

        return !redisResult.IsNull;
    }

    public bool TryGet<T>(string key, out T value)
    {
        Connect();

        var redisKey = ModifyKey(key);
        var redisCommand = RedisCommand.CreateGetCommand(redisKey);
        var redisResult = EvaluateCommand(redisCommand);

        value = default;

        if (redisResult.IsNull)
        {
            return false;
        }

        value = RedisTypeConverter.ToObject<T>(redisResult, _serializer);

        return true;
    }

    public async Task<(bool found, T value)> TryGetAsync<T>(string key)
    {
        await ConnectAsync().ConfigureAwait(false);

        var redisKey = ModifyKey(key);
        var redisCommand = RedisCommand.CreateGetCommand(redisKey);
        var redisResult = await EvaluateCommandAsync(redisCommand).ConfigureAwait(false);
        var value = default(T);

        if (redisResult.IsNull)
        {
            return (false, value);
        }

        value = RedisTypeConverter.ToObject<T>(redisResult, _serializer);

        return (true, value);
    }

    public void Remove(string key)
    {
        var redisKey = ModifyKey(key);

        Connect();
        _database.KeyDelete(redisKey);
    }

    public async Task RemoveAsync(string key)
    {
        var redisKey = ModifyKey(key);

        await ConnectAsync().ConfigureAwait(false);
        await _database.KeyDeleteAsync(redisKey).ConfigureAwait(false);
    }

    public void Set<T>(string key, T value, CacheItemOptions options)
    {
        Connect();

        var redisKey = ModifyKey(key);
        var redisValue = RedisTypeConverter.ToRedisValue(value, _serializer);
        var redisCommand = RedisCommand.CreateSetCommand(
            redisKey,
            redisValue,
            options,
            _clock.UtcNow);

        EvaluateCommand(redisCommand);
    }

    public async Task SetAsync<T>(string key, T value, CacheItemOptions options)
    {
        await ConnectAsync().ConfigureAwait(false);

        var redisKey = ModifyKey(key);
        var redisValue = RedisTypeConverter.ToRedisValue(value, _serializer);
        var redisCommand = RedisCommand.CreateSetCommand(
            redisKey,
            redisValue,
            options,
            _clock.UtcNow);

        await EvaluateCommandAsync(redisCommand).ConfigureAwait(false);
    }

    public async Task<long> StringIncrementAsync(string key)
    {
        await ConnectAsync().ConfigureAwait(false);
        try
        {
            return await _database.StringIncrementAsync(key);
        }
        catch (RedisServerException e) when (e.Message.Contains("READONLY"))
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
        catch (Exception e) when (e is RedisConnectionException or SocketException)
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<bool> StringSetAsync(string key, string value)
    {
        await ConnectAsync().ConfigureAwait(false);
        try
        {
            return await _database.StringSetAsync(key, value);
        }
        catch (RedisServerException e) when (e.Message.Contains("READONLY"))
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
        catch (Exception e) when (e is RedisConnectionException or SocketException)
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<string> StringGetAsync(string key)
    {
        await ConnectAsync().ConfigureAwait(false);
        try
        {
            return await _database.StringGetAsync(key);
        }
        catch (RedisServerException e) when (e.Message.Contains("READONLY"))
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
        catch (Exception e) when (e is RedisConnectionException or SocketException)
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
    }

    private RedisResult EvaluateCommand(RedisCommand redisCommand)
    {
        try
        {
            return _database.ScriptEvaluate(redisCommand.Script, redisCommand.Keys, redisCommand.Values);
        }
        catch (RedisServerException e) when (e.Message.Contains("READONLY"))
        {
            ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
        catch (Exception e) when (e is RedisConnectionException or SocketException)
        {
            ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
    }


    private async Task<RedisResult> EvaluateCommandAsync(RedisCommand redisCommand)
    {
        try
        {
            return await _database.ScriptEvaluateAsync(redisCommand.Script, redisCommand.Keys, redisCommand.Values);
        }
        catch (RedisServerException e) when (e.Message.Contains("READONLY"))
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
        catch (Exception e) when (e is RedisConnectionException or SocketException)
        {
            await ForceReconnectAsync().ConfigureAwait(false);
            throw;
        }
    }


    private void Connect()
    {
        if (_database != null)
        {
            return;
        }

        _connectionLock.Wait();

        try
        {
            if (_database != null)
            {
                return;
            }

            _connection = ConnectionMultiplexer.Connect(_redisConnectionOptions);
            _database = _connection.GetDatabase();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task ConnectAsync()
    {
        if (_database != null)
        {
            return;
        }

        await _connectionLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_database == null)
            {
                _connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionOptions).ConfigureAwait(false);
                _database = _connection.GetDatabase();
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private string ModifyKey(string key)
        => $"{{{_applicationName}}}:{{{key}}}";

    public void Dispose()
    {
        _connectionLock.Dispose();
        _connection?.Dispose();
        _connection = null;
    }

}