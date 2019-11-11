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
        private readonly CacheItemOptions _testingCacheItemOptions;
        private readonly string _testingKey;
        private readonly object _testingValue;
        private readonly object _newlyCreatedValue;

        public ConfiguredCacheServiceTests()
        {
            var fixture = new Fixture();

            _cacheService = Substitute.For<ICacheService>();
            _testingCacheItemOptions = new CacheItemOptions();
            _testingKey = fixture.Create<string>();
            _testingValue = fixture.Create<object>();
            _newlyCreatedValue = fixture.Create<object>();

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
        public async Task ContainAsyncPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.ContainAsync(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .ContainAsync(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void GetPropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Get<object>(_testingKey);

            // Assert.
            _cacheService.Received().Get<object>(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task GetAsyncPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.GetAsync<object>(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .GetAsync<object>(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void TryGetPropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.TryGet(_testingKey, out object _);

            // Assert.
            _cacheService.Received().TryGet(Arg.Is(_testingKey), out object _);
        }

        [Fact]
        public async Task TryGetAsyncPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.TryGetAsync<object>(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .TryGetAsync<object>(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void GetOrSetPropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.GetOrSet(_testingKey, CreateValueFakely, _testingCacheItemOptions);

            // Assert.
            _cacheService
                .Received()
                .GetOrSet(Arg.Is(_testingKey), Arg.Is<Func<object>>(CreateValueFakely), Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public void GetOrSetWithNoCacheItemOptionsPropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.GetOrSet(_testingKey, CreateValueFakely);

            // Assert.
            _cacheService
                .Received()
                .GetOrSet(Arg.Is(_testingKey), Arg.Is<Func<object>>(CreateValueFakely));
        }

        [Fact]
        public async Task GetOrSetAsyncPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService
                .GetOrSetAsync(_testingKey, CreateValueFakelyAsync, _testingCacheItemOptions)
                .ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .GetOrSetAsync(Arg.Is(_testingKey), Arg.Is<Func<Task<object>>>(CreateValueFakelyAsync), Arg.Is(_testingCacheItemOptions))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetOrSetAsyncWithNoCacheItemOptionsPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService
                .GetOrSetAsync(_testingKey, CreateValueFakelyAsync)
                .ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .GetOrSetAsync(Arg.Is(_testingKey), Arg.Is<Func<Task<object>>>(CreateValueFakelyAsync))
                .ConfigureAwait(false);
        }

        [Fact]
        public void RemovePropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Remove(_testingKey);

            // Assert.
            _cacheService.Received().Remove(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task RemoveAsyncPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.RemoveAsync(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .RemoveAsync(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void SetPropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Set(_testingKey, _testingValue, _testingCacheItemOptions);

            // Assert.
            _cacheService
                .Received()
                .Set(Arg.Is(_testingKey), Arg.Is(_testingValue), Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public void SetWithNoCacheItemOptionsPropagatesCallToRegisteredCacheService()
        {
            // Act.
            ConfiguredCacheService.Set(_testingKey, _testingValue);

            // Assert.
            _cacheService.Received().Set(Arg.Is(_testingKey), Arg.Is(_testingValue));
        }

        [Fact]
        public async Task SetAsyncPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService
                .SetAsync(_testingKey, _testingValue, _testingCacheItemOptions)
                .ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .SetAsync(Arg.Is(_testingKey), Arg.Is(_testingValue), Arg.Is(_testingCacheItemOptions))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SetAsyncWithNoCacheItemOptionsPropagatesCallToRegisteredCacheService()
        {
            // Act.
            await ConfiguredCacheService.SetAsync(_testingKey, _testingValue).ConfigureAwait(false);

            // Assert.
            await _cacheService
                .Received()
                .SetAsync(Arg.Is(_testingKey), Arg.Is(_testingValue))
                .ConfigureAwait(false);
        }

        private object CreateValueFakely()
            => _newlyCreatedValue;

        private Task<object> CreateValueFakelyAsync()
            => Task.FromResult(_newlyCreatedValue);
    }
}