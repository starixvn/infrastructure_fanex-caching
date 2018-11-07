namespace Fanex.Caching.Redis.Tests
{
    using System.Linq;
    using StackExchange.Redis;
    using Xunit;

    public class RedisCacheOptionsTests
    {
        [Fact]
        public void TakeMessagePackAsDefaultSerializer()
        {
            // Act.
            var redisCacheOptions = new RedisCacheOptions();

            // Assert.
            Assert.IsType<MessagePackSerializer>(redisCacheOptions.Serializer);
        }

        [Fact]
        public void ParseRedisConfigurationOptionsFromConnectionString()
        {
            // Arrange.
            var connectionString = "10.18.200.110:6379";
            var redisCacheOptions = new RedisCacheOptions
            {
                ConnectionString = connectionString
            };

            // Act.
            var configurationOptions = redisCacheOptions.ToRedisConnectionOptions();

            // Assert.
            Assert.Single(configurationOptions.EndPoints);

            var createdEndPoint = configurationOptions.EndPoints.ElementAt(0);

            Assert.Equal(connectionString, createdEndPoint.ToString());
        }

        [Fact]
        public void PrioritizeConnectionOptionsOverConnectionString()
        {
            // Arrange.
            var connectionOptions = new ConfigurationOptions();
            var redisCacheOptions = new RedisCacheOptions
            {
                ConnectionOptions = connectionOptions
            };

            connectionOptions.EndPoints.Add("http://localhost:6379/");

            // Act.
            var actualConfigurationOptions = redisCacheOptions.ToRedisConnectionOptions();

            // Assert.
            Assert.Same(connectionOptions, actualConfigurationOptions);
        }
    }
}