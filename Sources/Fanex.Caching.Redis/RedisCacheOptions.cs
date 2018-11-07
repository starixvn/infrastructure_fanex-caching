namespace Fanex.Caching.Redis
{
    using System;
    using StackExchange.Redis;

    public class RedisCacheOptions
    {
        private ISystemClock _clock = new SystemClock();
        private ISerializer _serializer = new MessagePackSerializer();

        public string ConnectionString { get; set; }

        public ConfigurationOptions ConnectionOptions { get; set; }

        public string ApplicationName { get; set; }

        public ISystemClock Clock
        {
            get => _clock;
            set => _clock = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ISerializer Serializer
        {
            get => _serializer;
            set => _serializer = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal ConfigurationOptions ToRedisConnectionOptions()
        {
            if (ConnectionOptions != null)
            {
                return ConnectionOptions;
            }

            return ConfigurationOptions.Parse(ConnectionString, true);
        }
    }
}