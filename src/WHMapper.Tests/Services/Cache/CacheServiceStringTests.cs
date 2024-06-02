using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;
using WHMapper.Services.Cache;

namespace WHMapper.Tests.Services.Cache
{
    public class CacheServiceStringTests
    {
        [Theory, AutoDomainData]
        public async Task WhenCacheContainsSerializedByteArray_GettingKey_ReturnsValue(
            string key,
            string cachedValue,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            var serializedCachedValue = JsonSerializer.Serialize(cachedValue);
            byte[] cachedValueBytes = Encoding.ASCII.GetBytes(serializedCachedValue);
            cacheMock.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(cachedValueBytes);

            var result = await sut.Get<string>(key);

            Assert.Equal(cachedValue, result);
        }

        [Theory, AutoDomainData]
        public async Task WhenCacheContainsDeformedString_GettingKey_ReturnsNull(
            string key,
            string value,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            byte[] valueBytes = Encoding.ASCII.GetBytes(value);
            cacheMock.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(valueBytes);

            var result = await sut.Get<string>(key);

            Assert.Null(result);
        }

        [Theory, AutoDomainData]
        public async Task WhenCacheDoesntContainKey_ServiceReturnsNull(
            string key,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            cacheMock.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((byte[]?)null);

            var result = await sut.Get<string>(key);

            Assert.Null(result);
        }

        [Theory, AutoMoqData]
        public async Task WhenSettingValue_WithExpiry_SetsValue(
            string key,
            string value,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            byte[] serializedValueBytes = Encoding.ASCII.GetBytes(serializedValue);

            var result = await sut.Set(key, value, new TimeSpan(0,5,0));

            Assert.True(result);
            cacheMock.Verify(cache => cache.SetAsync(key, serializedValueBytes, It.Is<DistributedCacheEntryOptions>(x => x.AbsoluteExpirationRelativeToNow != null), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task WhenSettingValue_WithoutExpiry_SetsValue(
            string key,
            string value,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            byte[] serializedValueBytes = Encoding.ASCII.GetBytes(serializedValue);

            var result = await sut.Set(key, value);

            Assert.True(result);
            cacheMock.Verify(cache => cache.SetAsync(key, serializedValueBytes, It.Is<DistributedCacheEntryOptions>(x => x.AbsoluteExpirationRelativeToNow == null), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task WhenSettingValue_WhenKeyIsNull_ReturnsFalse(
            string value,
            CacheService sut)
        {
            var result = await sut.Set(null!, value);
            Assert.False(result);
        }

        [Theory, AutoMoqData]
        public async Task WhenSettingValue_ThatExists_OverWritesValue(
            string key,
            string value,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            byte[] serializedValueBytes = Encoding.ASCII.GetBytes(serializedValue);

            //Ensuring that the IDistributedCache always returns something, this is to
            //ensure that this behaviour doesn't change in the future.
            cacheMock.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(serializedValueBytes);
            cacheMock.Setup(cache => cache.Get(key))
                    .Returns(serializedValueBytes);

            var result = await sut.Set(key, value);

            Assert.True(result);
            cacheMock.Verify(cache => cache.SetAsync(key, serializedValueBytes, It.Is<DistributedCacheEntryOptions>(x => x.AbsoluteExpirationRelativeToNow == null), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task WhenRemovingValue_RemovesValue(
            string key,
            [Frozen] Mock<IDistributedCache> cacheMock,
            CacheService sut)
        {
            var result = await sut.Remove(key);

            Assert.True(result);
            cacheMock.Verify(cache => cache.RemoveAsync(key, default), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task WhenRemovingValue_KeyIsNull_ReturnsFalse(
            CacheService sut)
         {
            var result = await sut.Remove(null!);
            Assert.False(result);
        }
    }
}