namespace Fanex.Caching.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using NSubstitute;
    using Xunit;

    public class CacheServiceTests
    {
        private readonly ICache _cacheStorage;
        private readonly CacheService _cacheService;
        private readonly CacheItemOptions _testingCacheItemOptions;
        private readonly string _testingKey;
        private readonly object _testingValue;
        private readonly Func<object> _testingFactory;

        public CacheServiceTests()
        {
            var fixture = new Fixture();

            _cacheStorage = Substitute.For<ICache>();
            _cacheService = new CacheService(_cacheStorage);
            _testingCacheItemOptions = fixture.Create<CacheItemOptions>();
            _testingKey = fixture.Create<string>();
            _testingValue = fixture.Create<object>();
            _testingFactory = fixture.Create<Func<object>>();
        }

        [Fact]
        public void ContainAlwaysCallCacheStorageContain()
        {
            // Act.
            _cacheService.Contain(_testingKey);

            // Assert.
            _cacheStorage.Received().Contain(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task ContainAsyncAlwaysCallCacheStorageContainAsync()
        {
            // Act.
            await _cacheService.ContainAsync(_testingKey);

            // Assert.
            await _cacheStorage.Received().ContainAsync(Arg.Is(_testingKey));
        }

        [Fact]
        public void GetAlwaysCallCacheStorageTryGet()
        {
            // Act.
            _cacheService.Get<object>(_testingKey);

            // Assert.
            _cacheStorage.Received().TryGet<object>(Arg.Is(_testingKey), out _);
        }

        [Fact]
        public void GetReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            object result = _cacheService.Get<object>(_testingKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = _testingValue;

                    return true;
                });

            // Act.
            object result = _cacheService.Get<object>(_testingKey);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public async Task GetAsyncAlwaysCallCacheStorageTryGetAsync()
        {
            // Act.
            await _cacheService.GetAsync<object>(_testingKey);

            // Assert.
            await _cacheStorage.Received().TryGetAsync<object>(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task GetAsyncReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetAsync<object>(_testingKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(true, _testingValue));

            // Act.
            var result = await _cacheService.GetAsync<object>(_testingKey);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public void TryGetAlwaysCallCacheStorageTryGet()
        {
            // Act.
            _cacheService.TryGet<object>(_testingKey, out _);

            // Assert.
            _cacheStorage.Received().TryGet<object>(Arg.Is(_testingKey), out _);
        }

        [Fact]
        public async Task TryGetAsyncAlwaysCallCacheStorageTryGetAsync()
        {
            // Act.
            await _cacheService.TryGetAsync<object>(_testingKey);

            // Assert.
            await _cacheStorage.Received().TryGetAsync<object>(Arg.Is(_testingKey));
        }

        [Fact]
        public void GetOrSetExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = _testingValue;

                    return true;
                });

            // Act.
            object result = _cacheService.GetOrSet(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            object result = _cacheService.GetOrSet<object>(_testingKey, null, _testingCacheItemOptions);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetOrSetMissingKeyStoresValueCreatedByFactoryThenReturnsIt()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            object result = _cacheService.GetOrSet(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(_testingKey),
                    Arg.Is(newValue),
                    Arg.Is(_testingCacheItemOptions));

            Assert.Same(newValue, result);
        }

        [Fact]
        public void GetOrSetExistingKeyWithoutCacheItemOptionsReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = _testingValue;

                    return true;
                });

            // Act.
            object result = _cacheService.GetOrSet(_testingKey, _testingFactory);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithoutCacheItemOptionsCachesValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage
                .TryGet<object>(_testingKey, out _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            _cacheService.GetOrSet(_testingKey, _testingFactory);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(_testingKey),
                    Arg.Is(newValue),
                    Arg.Is<CacheItemOptions>(testingOptions => testingOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }

        [Fact]
        public async Task GetOrSetAsyncExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(true, _testingValue));

            // Act.
            object result = await _cacheService.GetOrSetAsync(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetOrSetAsync<object>(_testingKey, null, _testingCacheItemOptions);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyStoresValueCreatedByFactoryThenReturnsIt()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetOrSetAsync(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(newValue),
                    Arg.Is(_testingCacheItemOptions));

            Assert.Same(newValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncExistingKeyWithoutCacheItemOptionsReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(true, _testingValue));

            // Act.
            object result = await _cacheService.GetOrSetAsync(_testingKey, _testingFactory);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyWithoutCacheItemOptionsCachesValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage.TryGetAsync<object>(_testingKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            await _cacheService.GetOrSetAsync(_testingKey, _testingFactory);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(newValue),
                    Arg.Is<CacheItemOptions>(testingOptions => testingOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }

        [Fact]
        public void RemoveAlwaysCallCacheStorageRemove()
        {
            // Act.
            _cacheService.Remove(_testingKey);

            // Assert.
            _cacheStorage.Received().Remove(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task RemoveAsyncAlwaysCallCacheStorageRemoveAsync()
        {
            // Act.
            await _cacheService.RemoveAsync(_testingKey);

            // Assert.
            await _cacheStorage.Received().RemoveAsync(Arg.Is(_testingKey));
        }

        [Fact]
        public void SetAlwaysCallCacheStorageSet()
        {
            // Act.
            _cacheService.Set(_testingKey, _testingValue, _testingCacheItemOptions);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public void SetWithoutCacheItemOptionsCachesValueWithDefaultRelativeExpiration()
        {
            // Act.
            _cacheService.Set(_testingKey, _testingValue);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is<CacheItemOptions>(testingOptions => testingOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }

        [Fact]
        public async Task SetAsyncAlwaysCallCacheStorageSetAsync()
        {
            // Act.
            await _cacheService.SetAsync(_testingKey, _testingValue, _testingCacheItemOptions);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public async Task SetAsyncWithoutCacheItemOptionsCachesValueWithDefaultRelativeExpiration()
        {
            // Act.
            await _cacheService.SetAsync(_testingKey, _testingValue);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is<CacheItemOptions>(testingOptions => testingOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }
    }
}