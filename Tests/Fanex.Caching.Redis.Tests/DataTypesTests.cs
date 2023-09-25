namespace Fanex.Caching.Redis.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public sealed class DataTypesTests : IClassFixture<RedisCacheFixture>, IDisposable
    {
        private readonly Fixture _fixture;
        private readonly string _testingKey;
        private readonly CacheItemOptions _cacheOptions;

        public DataTypesTests(RedisCacheFixture cacheFixture)
        {
            _fixture = new Fixture();
            _testingKey = _fixture.Create<string>();
            _cacheOptions = new CacheItemOptions().UseDefaultExpiration();

            Cache = cacheFixture.Cache;
        }

        private RedisCache Cache { get; }

        [Fact]
        public async Task CacheString()
        {
            // Arrange.
            var anyString = _fixture.Create<string>();

            // Act.
            await Cache.SetAsync(_testingKey, anyString, _cacheOptions);

            // Assert.
            var (_, cachedString) = await Cache.TryGetAsync<string>(_testingKey);

            Assert.Equal(anyString, cachedString);
        }

        [Fact]
        public async Task CacheBool()
        {
            // Arrange.
            var anyBool = _fixture.Create<bool>();

            // Act.
            await Cache.SetAsync(_testingKey, anyBool, _cacheOptions);

            // Assert.
            var (_, cachedBool) = await Cache.TryGetAsync<bool>(_testingKey);

            Assert.Equal(anyBool, cachedBool);
        }

        [Fact]
        public async Task CacheByte()
        {
            // Arrange.
            var anyByte = _fixture.Create<byte>();

            // Act.
            await Cache.SetAsync(_testingKey, anyByte, _cacheOptions);

            // Assert.
            var (_, cachedByte) = await Cache.TryGetAsync<byte>(_testingKey);

            Assert.Equal(anyByte, cachedByte);
        }

        [Fact]
        public async Task CacheShort()
        {
            // Arrange.
            var anyShort = _fixture.Create<short>();

            // Act.
            await Cache.SetAsync(_testingKey, anyShort, _cacheOptions);

            // Assert.
            var (_, cachedShort) = await Cache.TryGetAsync<short>(_testingKey);

            Assert.Equal(anyShort, cachedShort);
        }

        [Fact]
        public async Task CacheInt()
        {
            // Arrange.
            var anyInt = _fixture.Create<int>();

            // Act.
            await Cache.SetAsync(_testingKey, anyInt, _cacheOptions);

            // Assert.
            var (_, cachedInt) = await Cache.TryGetAsync<int>(_testingKey);

            Assert.Equal(anyInt, cachedInt);
        }

        [Fact]
        public async Task CacheLong()
        {
            // Arrange.
            var anyLong = _fixture.Create<long>();

            // Act.
            await Cache.SetAsync(_testingKey, anyLong, _cacheOptions);

            // Assert.
            var (_, cachedLong) = await Cache.TryGetAsync<long>(_testingKey);

            Assert.Equal(anyLong, cachedLong);
        }

        [Fact]
        public async Task CacheFloat()
        {
            // Arrange.
            var anyFloat = _fixture.Create<float>();

            // Act.
            await Cache.SetAsync(_testingKey, anyFloat, _cacheOptions);

            // Assert.
            var (_, cachedFloat) = await Cache.TryGetAsync<float>(_testingKey);

            Assert.Equal(anyFloat, cachedFloat);
        }

        [Fact]
        public async Task CacheDouble()
        {
            // Arrange.
            var anyDouble = _fixture.Create<double>();

            // Act.
            await Cache.SetAsync(_testingKey, anyDouble, _cacheOptions);

            // Assert.
            var (_, cachedDouble) = await Cache.TryGetAsync<double>(_testingKey);

            Assert.Equal(anyDouble, cachedDouble);
        }

        [Fact]
        public async Task CacheDecimal()
        {
            // Arrange.
            var anyDecimal = _fixture.Create<decimal>();

            // Act.
            await Cache.SetAsync(_testingKey, anyDecimal, _cacheOptions);

            // Assert.
            var (_, cachedDecimal) = await Cache.TryGetAsync<decimal>(_testingKey);

            Assert.Equal(anyDecimal, cachedDecimal);
        }

        [Fact]
        public async Task CacheNullableType()
        {
            // Arrange.
            var anyNullableInt = _fixture.Create<int?>();

            // Act.
            await Cache.SetAsync(_testingKey, anyNullableInt, _cacheOptions);

            // Assert.
            var (_, cachedNullableInt) = await Cache.TryGetAsync<int?>(_testingKey);

            Assert.Equal(anyNullableInt, cachedNullableInt);
        }

        [Fact]
        public async Task CacheBinary()
        {
            // Arrange.
            var anyBinary = _fixture.Create<byte[]>();

            // Act.
            await Cache.SetAsync(_testingKey, anyBinary, _cacheOptions);

            // Assert.
            var (_, cachedBinary) = await Cache.TryGetAsync<byte[]>(_testingKey);

            Assert.Equal(anyBinary, cachedBinary);
        }

        [Fact]
        public async Task CacheObject()
        {
            // Arrange.
            var anyObject = new Dictionary<string, string>
            {
                [_fixture.Create<string>()] = _fixture.Create<string>()
            };

            // Act.
            await Cache.SetAsync(_testingKey, anyObject, _cacheOptions);

            // Assert.
            var (_, cachedObject) = await Cache.TryGetAsync<Dictionary<string, string>>(_testingKey);

            Assert.Equal(anyObject, cachedObject);
        }

        public void Dispose()
            => Cache.Remove(_testingKey);
    }
}