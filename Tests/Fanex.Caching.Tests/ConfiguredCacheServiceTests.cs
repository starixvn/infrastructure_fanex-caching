namespace Fanex.Caching.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using NSubstitute;
    using Xunit;

    public class ConfiguredCacheServiceTests
    {
        private readonly ICacheService _cacheService;
        private readonly string _testingKey;
        private readonly object _testingValue;
        private readonly CacheItemOptions _testingCacheItemOptions;
        private readonly Func<object> _testingFactory;

        public ConfiguredCacheServiceTests()
        {
            var fixture = new Fixture();

            _cacheService = Substitute.For<ICacheService>();
            _testingKey = fixture.Create<string>();
            _testingValue = fixture.Create<object>();
            _testingCacheItemOptions = fixture.Create<CacheItemOptions>();
            _testingFactory = fixture.Create<Func<object>>();

            ConfiguredCacheService.Initialize(_cacheService);
        }

        [Fact]
        public void TakeCacheStorageFromCacheServiceManager()
        {
            // Arrange
            var configuredCacheStorage = Substitute.For<ICache>();

            CacheServiceManager.Reset();
            CacheServiceManager.Use(configuredCacheStorage);
            ConfiguredCacheService.Initialize(null);

            // Act.
            var currentStorage = ConfiguredCacheService.CacheStorage;

            // Assert.
            Assert.Same(configuredCacheStorage, currentStorage);

            CacheServiceManager.Reset();
        }

        [Fact]
        public void ContainAlwaysCallsContainOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Contain(_testingKey);

            // Assert.
            _cacheService.Received().Contain(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task ContainAsyncAlwaysCallsContainAsyncOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.ContainAsync(_testingKey);

            // Assert.
            await _cacheService.Received().ContainAsync(Arg.Is(_testingKey));
        }

        [Fact]
        public void GetAlwaysCallsGetOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Get<object>(_testingKey);

            // Assert.
            _cacheService.Received().Get<object>(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task GetAsyncAlwaysCallsGetAsyncOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.GetAsync<object>(_testingKey);

            // Assert.
            await _cacheService.Received().GetAsync<object>(Arg.Is(_testingKey));
        }

        [Fact]
        public void TryGetAlwaysCallsTryGetOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.TryGet<object>(_testingKey, out _);

            // Assert.
            _cacheService.Received().TryGet<object>(Arg.Is(_testingKey), out _);
        }

        [Fact]
        public async Task TryGetAsyncAlwaysCallsTryGetAsyncOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.TryGetAsync<object>(_testingKey);

            // Assert.
            await _cacheService.Received().TryGetAsync<object>(Arg.Is(_testingKey));
        }

        [Fact]
        public void GetOrSetAlwaysCallsGetOrSetOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.GetOrSet(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            _cacheService
                .Received()
                .GetOrSet(
                    Arg.Is(_testingKey),
                    Arg.Any<Func<object>>(),
                    Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public void GetOrSetNotTakingCacheItemOptionsAlwaysCallsGetOrSetNotTakingCacheItemOptionsOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.GetOrSet(_testingKey, _testingFactory);

            // Assert.
            _cacheService.Received().GetOrSet(Arg.Is(_testingKey), Arg.Any<Func<object>>());
        }

        [Fact]
        public async Task GetOrSetAsyncAlwaysCallsGetOrSetAsyncOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.GetOrSetAsync(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            await _cacheService
                .Received()
                .GetOrSetAsync(
                    Arg.Is(_testingKey),
                    Arg.Any<Func<object>>(),
                    Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public async Task GetOrSetAsyncNotTakingCacheItemOptionsAlwaysCallsGetOrSetAsyncNotTakingCacheItemOptionsOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.GetOrSetAsync(_testingKey, _testingFactory);

            // Assert.
            await _cacheService.Received().GetOrSetAsync(Arg.Is(_testingKey), Arg.Any<Func<object>>());
        }

        [Fact]
        public void RemoveAlwaysCallsRemoveOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Remove(_testingKey);

            // Assert.
            _cacheService.Received().Remove(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task RemoveAsyncAlwaysCallsRemoveAsyncOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.RemoveAsync(_testingKey);

            // Assert.
            await _cacheService.Received().RemoveAsync(Arg.Is(_testingKey));
        }

        [Fact]
        public void SetAlwaysCallsSetOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Set(_testingKey, _testingValue, _testingCacheItemOptions);

            // Assert.
            _cacheService
                .Received()
                .Set(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public void SetNotTakingCacheItemOptionsAlwaysCallsSetNotTakingCacheItemOptionsOfRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Set(_testingKey, _testingValue);

            // Assert.
            _cacheService.Received().Set(Arg.Is(_testingKey), Arg.Is(_testingValue));
        }

        [Fact]
        public async Task SetAsyncAlwaysCallsSetAsyncOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.SetAsync(_testingKey, _testingValue, _testingCacheItemOptions);

            // Assert.
            await _cacheService
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public async Task SetAsyncNotTakingCacheItemOptionsAlwaysCallsSetAsyncNotTakingCacheItemOptionsOfRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.SetAsync(_testingKey, _testingValue);

            // Assert.
            await _cacheService.Received().SetAsync(Arg.Is(_testingKey), Arg.Is(_testingValue));
        }
    }
}