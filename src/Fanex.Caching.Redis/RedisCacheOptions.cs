namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    public class RedisCacheOptions
    {
        private const string DefaultHost = "127.0.0.1";
        private const int DefaultPort = 6379;
        private const int DefaultOperationTimeout = 1000;
        private const int DefaultConnectionTimeout = 5000;

        public Serializer Serializer { get; set; }
            = new MessagePackSerializer();

        public string ApplicationName { get; set; }
            = string.Empty;

        public string ConnectionString { get; set; }

        public string Host { get; set; }
            = DefaultHost;

        public int Port { get; set; }
            = DefaultPort;

        public bool UseSsl { get; set; }
            = false;

        public int OperationTimeout { get; set; }
            = DefaultOperationTimeout;

        public int ConnectionTimeout { get; set; }
            = DefaultConnectionTimeout;

        public string Password { get; set; }

        internal ConfigurationOptions ToRedisConnectionOptions()
        {
            ConfigurationOptions redisConnectionOptions;

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                redisConnectionOptions = ConfigurationOptions.Parse(ConnectionString);
            }
            else
            {
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