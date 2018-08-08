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
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), $"{nameof(Clock)} is required.");
                }

                _clock = value;
            }
        }

        public ISerializer Serializer
        {
            get => _serializer;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), $"{nameof(Serializer)} is required.");
                }

                _serializer = value;
            }
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