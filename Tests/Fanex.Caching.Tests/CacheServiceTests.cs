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
        public void ContainCallsContainOfCacheStorage()
        {
            // Act.
            _cacheService.Contain(_testingKey);

            // Assert.
            _cacheStorage.Received().Contain(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task ContainAsyncCallsContainAsyncOfCacheStorage()
        {
            // Act.
            await _cacheService.ContainAsync(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .ContainAsync(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void GetCallsTryGetOfCacheStorage()
        {
            // Act.
            _cacheService.Get<object>(_testingKey);

            // Assert.
            _cacheStorage.Received().TryGet(Arg.Is(_testingKey), out object _);
        }

        [Fact]
        public void GetReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            _cacheStorage
                .TryGet(_testingKey, out object _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            var result = _cacheService.Get<object>(_testingKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage
                .TryGet(_testingKey, out object _)
                .Returns(parameters =>
                {
                    parameters[1] = _testingValue;

                    return true;
                });

            // Act.
            var result = _cacheService.Get<object>(_testingKey);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public async Task GetAsyncCallsTryGetAsyncOfCacheStorage()
        {
            // Act.
            await _cacheService.GetAsync<object>(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .TryGetAsync<object>(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetAsyncReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((false, null));

            // Act.
            var result = await _cacheService.GetAsync<object>(_testingKey).ConfigureAwait(false);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((true, _testingValue));

            // Act.
            var result = await _cacheService.GetAsync<object>(_testingKey).ConfigureAwait(false);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public void TryGetCallsTryGetOfCacheStorage()
        {
            // Act.
            _cacheService.TryGet(_testingKey, out object _);

            // Assert.
            _cacheStorage.Received().TryGet(Arg.Is(_testingKey), out object _);
        }

        [Fact]
        public async Task TryGetAsyncCallsTryGetAsyncOfCacheStorage()
        {
            // Act.
            await _cacheService.TryGetAsync<object>(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .TryGetAsync<object>(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void GetOrSetExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage
                .TryGet(_testingKey, out object _)
                .Returns(parameters =>
                {
                    parameters[1] = _testingValue;

                    return true;
                });

            // Act.
            var result = _cacheService.GetOrSet(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            _cacheStorage
                .TryGet(_testingKey, out object _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            var result = _cacheService.GetOrSet<object>(_testingKey, null, _testingCacheItemOptions);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetOrSetMissingKeyStoresValueCreatedByFactoryThenReturnsIt()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage
                .TryGet(_testingKey, out object _)
                .Returns(parameters =>
                {
                    parameters[1] = null;

                    return false;
                });

            // Act.
            var result = _cacheService.GetOrSet(_testingKey, _testingFactory, _testingCacheItemOptions);

            // Assert.
            _cacheStorage
                .Received()
                .Set(Arg.Is(_testingKey), Arg.Is(newValue), Arg.Is(_testingCacheItemOptions));

            Assert.Same(newValue, result);
        }

        [Fact]
        public void GetOrSetExistingKeyWithNoCacheItemOptionsReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage
                .TryGet(_testingKey, out object _)
                .Returns(parameters =>
                {
                    parameters[1] = _testingValue;

                    return true;
                });

            // Act.
            var result = _cacheService.GetOrSet(_testingKey, _testingFactory);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithNoCacheItemOptionsStoresValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage
                .TryGet(_testingKey, out object _)
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
                    Arg.Is<CacheItemOptions>(testingOptions => IsUsingDefaultRelativeExpiration(testingOptions)));
        }

        [Fact]
        public async Task GetOrSetAsyncExistingKeyReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((true, _testingValue));

            // Act.
            var result = await _cacheService
                .GetOrSetAsync(_testingKey, _testingFactory, _testingCacheItemOptions)
                .ConfigureAwait(false);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((false, null));

            // Act.
            var result = await _cacheService
                .GetOrSetAsync<object>(_testingKey, null, _testingCacheItemOptions)
                .ConfigureAwait(false);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyStoresValueCreatedByFactoryThenReturnsIt()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((false, null));

            // Act.
            var result = await _cacheService
                .GetOrSetAsync(_testingKey, _testingFactory, _testingCacheItemOptions)
                .ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(Arg.Is(_testingKey), Arg.Is(newValue), Arg.Is(_testingCacheItemOptions))
                .ConfigureAwait(false);

            Assert.Same(newValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncExistingKeyWithNoCacheItemOptionsReturnsCachedValue()
        {
            // Arrange.
            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((true, _testingValue));

            // Act.
            var result = await _cacheService
                .GetOrSetAsync(_testingKey, _testingFactory)
                .ConfigureAwait(false);

            // Assert.
            Assert.Same(_testingValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyWithNoCacheItemOptionsStoresValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            var newValue = _testingFactory();

            _cacheStorage.TryGetAsync<object>(_testingKey).Returns((false, null));

            // Act.
            await _cacheService.GetOrSetAsync(_testingKey, _testingFactory).ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(newValue),
                    Arg.Is<CacheItemOptions>(testingOptions => IsUsingDefaultRelativeExpiration(testingOptions)))
                .ConfigureAwait(false);
        }

        [Fact]
        public void RemoveCallsRemoveOfCacheStorage()
        {
            // Act.
            _cacheService.Remove(_testingKey);

            // Assert.
            _cacheStorage.Received().Remove(Arg.Is(_testingKey));
        }

        [Fact]
        public async Task RemoveAsyncCallsRemoveAsyncOfCacheStorage()
        {
            // Act.
            await _cacheService.RemoveAsync(_testingKey).ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .RemoveAsync(Arg.Is(_testingKey))
                .ConfigureAwait(false);
        }

        [Fact]
        public void SetCallsSetOfCacheStorageSet()
        {
            // Act.
            _cacheService.Set(_testingKey, _testingValue, _testingCacheItemOptions);

            // Assert.
            _cacheStorage
                .Received()
                .Set(Arg.Is(_testingKey), Arg.Is(_testingValue), Arg.Is(_testingCacheItemOptions));
        }

        [Fact]
        public void SetWithNoCacheItemOptionsStoresValueWithDefaultRelativeExpiration()
        {
            // Act.
            _cacheService.Set(_testingKey, _testingValue);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is<CacheItemOptions>(testingOptions => IsUsingDefaultRelativeExpiration(testingOptions)));
        }

        [Fact]
        public async Task SetAsyncCallsSetAsyncOfCacheStorage()
        {
            // Act.
            await _cacheService
                .SetAsync(_testingKey, _testingValue, _testingCacheItemOptions)
                .ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(Arg.Is(_testingKey), Arg.Is(_testingValue), Arg.Is(_testingCacheItemOptions))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task SetAsyncWithNoCacheItemOptionsStoresValueWithDefaultRelativeExpiration()
        {
            // Act.
            await _cacheService.SetAsync(_testingKey, _testingValue).ConfigureAwait(false);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(_testingKey),
                    Arg.Is(_testingValue),
                    Arg.Is<CacheItemOptions>(testingOptions => IsUsingDefaultRelativeExpiration(testingOptions)))
                .ConfigureAwait(false);
        }

        private static bool IsUsingDefaultRelativeExpiration(CacheItemOptions options)
            => options.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration;
    }
}