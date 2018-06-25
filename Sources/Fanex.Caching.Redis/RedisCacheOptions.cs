namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

    public class RedisCacheOptions
    {
        private const int DefaultPort = 6379;
        private const int DefaultOperationTimeout = 1000;
        private const int DefaultConnectionTimeout = 5000;

        public ISerializer Serializer { get; set; }
            = new MessagePackSerializer();

        public string ApplicationName { get; set; }

        public string ConnectionString { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
            = DefaultPort;

        public bool UseSsl { get; set; }

        public int OperationTimeout { get; set; }
            = DefaultOperationTimeout;

        public int ConnectionTimeout { get; set; }
            = DefaultConnectionTimeout;

        public string Password { get; set; }

        internal ConfigurationOptions ToRedisConnectionOptions()
        {
            if (string.IsNullOrEmpty(ApplicationName))
            {
                throw new InvalidOperationException(
                    $"Application Name is required. It mus be configured via {nameof(RedisCacheOptions.ApplicationName)}.");
            }

            ConfigurationOptions redisConnectionOptions;

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                redisConnectionOptions = ConfigurationOptions.Parse(ConnectionString);
            }
            else
            {
                if (string.IsNullOrEmpty(Host))
                {
                    throw new InvalidOperationException(
                        $"Redis Host is required. It must be configured via {nameof(RedisCacheOptions.Host)}.");
                }

                redisConnectionOptions = new ConfigurationOptions();
                redisConnectionOptions.EndPoints.Add($"{Host}:{Port}");

                redisConnectionOptions.Password = Password;
                redisConnectionOptions.Ssl = UseSsl;
                redisConnectionOptions.AbortOnConnectFail = false;

                if (ConnectionTimeout != 0)
                {
                    redisConnectionOptions.ConnectTimeout = ConnectionTimeout;
                }

                if (OperationTimeout != 0)
                {
                    redisConnectionOptions.SyncTimeout = OperationTimeout;
                }
            }

            return redisConnectionOptions;
        }
    }
}