namespace Fanex.Caching.Tests.Configuration
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using Caching.Configuration;
    using Xunit;

    public class ConfigurationTests
    {
        private const string SectionName = "Fanex.Caching";

        private CacheSettings _cacheSettings;

        public ConfigurationTests()
        {
            // Arrange.
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "Configuration/ConfigurationTests.config"
            };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            _cacheSettings = config.GetSection(SectionName) as CacheSettings;
        }

        public static IEnumerable<object[]> ExpectedData
        {
            get
            {
                return new[]
                {
                    new object[] { "Host", "10.70.70.249" },
                    new object[] { "Port", "8000" },
                    new object[] { "Serializer", "Fanex.Caching.Redis.MessagePackSerializer, Fanex.Caching.Redis" }
                };
            }
        }

        [Fact]
        public void AlwaysContainsExpectedCacheTypeName()
        {
            // Assert.
            string expectedValue = "Fanex.Caching.Redis.RedisCache, Fanex.Caching.Redis";

            Assert.Equal(expectedValue, _cacheSettings.CacheType);
        }

        [Fact]
        public void ConnectionSettingCollectionHasThreeItems()
        {
            // Assert.
            Assert.Equal(3, _cacheSettings.ConnectionSettingCollection.Count);
        }

        [Theory, MemberData(nameof(ExpectedData))]
        public void ConnectionSettingCollectionContainsExpectedData(string name, string value)
        {
            // Act.
            Hashtable configuredData = _cacheSettings.ConnectionSettingCollection.ToHashtable();

            // Assert.
            Assert.True(configuredData.ContainsKey(name));
            Assert.Equal(value, configuredData[name]);
        }
    }
}