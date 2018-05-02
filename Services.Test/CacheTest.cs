// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class CacheTest
    {
        private Random rand = new Random();

        [Fact]
        public async Task GetCacheAsyncTestAsync()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();

            var cache = new Cache(
                mockStorageAdapterClient.Object,
                new Mock<IIothubManagerServiceClient>().Object,
                new Mock<ISimulationServiceClient>().Object,
                new ServicesConfig(),
                new Logger("UnitTest", LogLevel.Debug));

            var cacheValue = new CacheValue
            {
                Rebuilding = false,
                Tags = new HashSet<string> { "c", "a", "y", "z" },
                Reported = new HashSet<string> { "1", "9", "2", "3" }
            };

            mockStorageAdapterClient
                .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(cacheValue) }));

            var result = await cache.GetCacheAsync();

            Assert.True(result.Tags.OrderBy(m => m).SequenceEqual(cacheValue.Tags.OrderBy(m => m)));
            Assert.True(result.Reported.OrderBy(m => m).SequenceEqual(cacheValue.Reported.OrderBy(m => m)));
        }

        [Fact]
        public async Task SetCacheAsyncTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();

            var cache = new Cache(
                mockStorageAdapterClient.Object,
                new Mock<IIothubManagerServiceClient>().Object,
                new Mock<ISimulationServiceClient>().Object,
                new ServicesConfig(),
                new Logger("UnitTest", LogLevel.Debug));

            var oldCacheValue = new CacheValue
            {
                Rebuilding = false,
                Tags = new HashSet<string> { "c", "a", "y", "z" },
                Reported = new HashSet<string> { "1", "9", "2", "3" }
            };

            var cachePatch = new CacheValue
            {
                Tags = new HashSet<string> { "a", "y", "z", "@", "#" },
                Reported = new HashSet<string> { "9", "2", "3", "11", "12" }
            };

            var newCacheValue = new CacheValue
            {
                Tags = new HashSet<string> { "c", "a", "y", "z", "@", "#" },
                Reported = new HashSet<string> { "1", "9", "2", "3", "12", "11" }
            };

            mockStorageAdapterClient
                .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(oldCacheValue) }));

            mockStorageAdapterClient
                .Setup(m => m.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(newCacheValue) }));

            var result = await cache.SetCacheAsync(cachePatch);

            Assert.True(result.Tags.SetEquals(newCacheValue.Tags));
            Assert.True(result.Reported.SetEquals(newCacheValue.Reported));
        }

        [Fact]
        public async Task TryRebuildCacheAsyncSkipByTimeTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();

            var cache = new Cache(
                mockStorageAdapterClient.Object,
                new Mock<IIothubManagerServiceClient>().Object,
                new Mock<ISimulationServiceClient>().Object,
                new ServicesConfig
                {
                    CacheTTL = 60
                },
                new Logger("UnitTest", LogLevel.Debug));

            mockStorageAdapterClient
                .Setup(x => x.GetAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = this.rand.NextString(),
                    Data = JsonConvert.SerializeObject(new CacheValue
                    {
                        Rebuilding = false,
                        Tags = new HashSet<string> { "tags.IsSimulated" }
                    }),
                    Metadata = new Dictionary<string, string>
                    {
                        { "$modified", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture) }
                    }
                });

            var result = await cache.TryRebuildCacheAsync();
            Assert.False(result);

            mockStorageAdapterClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY)),
                    Times.Once);
        }

        [Fact]
        public async Task TryRebuildCacheAsyncSkipByConflictTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();

            var cache = new Cache(
                mockStorageAdapterClient.Object,
                new Mock<IIothubManagerServiceClient>().Object,
                new Mock<ISimulationServiceClient>().Object,
                new ServicesConfig
                {
                    CacheTTL = 10,
                    CacheRebuildTimeout = 300
                },
                new Logger("UnitTest", LogLevel.Debug));

            mockStorageAdapterClient
                .Setup(x => x.GetAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = this.rand.NextString(),
                    Data = JsonConvert.SerializeObject(new CacheValue
                    {
                        Rebuilding = true
                    }),
                    Metadata = new Dictionary<string, string>
                    {
                        { "$modified", (DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1)).ToString(CultureInfo.InvariantCulture) }
                    }
                });

            var result = await cache.TryRebuildCacheAsync();
            Assert.False(result);

            mockStorageAdapterClient
                .Verify(x => x.GetAsync(
                        It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                        It.Is<string>(s => s == Cache.CACHE_KEY)),
                    Times.Once);
        }

        [Fact]
        public async Task TryRebuildCacheAsyncTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            var mockIothubManagerClient = new Mock<IIothubManagerServiceClient>();
            var mockSimulationServiceClient = new Mock<ISimulationServiceClient>();

            var cache = new Cache(
                mockStorageAdapterClient.Object,
                mockIothubManagerClient.Object,
                mockSimulationServiceClient.Object,
                new ServicesConfig
                {
                    CacheWhiteList = "tags.*, reported.Type, reported.Config.*",
                    CacheTTL = 3600
                },
                new Logger("UnitTest", LogLevel.Debug));

            var etagOld = this.rand.NextString();
            var etagLock = this.rand.NextString();
            var etagNew = this.rand.NextString();

            mockStorageAdapterClient
                .Setup(x => x.GetAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = etagOld,
                    Data = JsonConvert.SerializeObject(new CacheValue
                    {
                        Rebuilding = false
                    }),
                    Metadata = new Dictionary<string, string>
                    {
                        { "$modified", (DateTimeOffset.UtcNow - TimeSpan.FromDays(1)).ToString(CultureInfo.InvariantCulture) }
                    }
                });

            mockIothubManagerClient
                .Setup(x => x.GetDeviceTwinNamesAsync())
                .ReturnsAsync(new DeviceTwinName
                {
                    Tags = new HashSet<string> { "Building", "Group" },
                    ReportedProperties = new HashSet<string> { "Config.Interval", "otherProperty" }
                });

            mockSimulationServiceClient
                .Setup(x => x.GetDevicePropertyNamesAsync())
                .ReturnsAsync(new HashSet<string>
                {
                    "MethodStatus", "UpdateStatus"
                });

            mockStorageAdapterClient
                .Setup(x => x.UpdateAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY),
                    It.Is<string>(s => Rebuilding(s)),
                    It.Is<string>(s => s == etagOld)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = etagLock
                });

            mockStorageAdapterClient
                .Setup(x => x.UpdateAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY),
                    It.Is<string>(s => !Rebuilding(s)),
                    It.Is<string>(s => s == etagLock)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = etagNew
                });

            var expiredNames = new DeviceTwinName
            {
                Tags = new HashSet<string>
                {
                    "Building", "Group"
                },
                ReportedProperties = new HashSet<string>
                {
                    "Type", "Config.Interval", "MethodStatus", "UpdateStatus"
                }
            };

            var result = await cache.TryRebuildCacheAsync();
            Assert.True(result);

            mockStorageAdapterClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY)),
                    Times.Once);

            mockIothubManagerClient
                .Verify(x => x.GetDeviceTwinNamesAsync(), Times.Once);

            mockSimulationServiceClient
                .Verify(x => x.GetDevicePropertyNamesAsync(), Times.Once);

            mockStorageAdapterClient
                .Verify(x => x.UpdateAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY),
                    It.Is<string>(s => Rebuilding(s)),
                    It.Is<string>(s => s == etagOld)),
                    Times.Once);

            mockStorageAdapterClient
                .Verify(x => x.UpdateAsync(
                    It.Is<string>(s => s == Cache.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == Cache.CACHE_KEY),
                    It.Is<string>(s => !Rebuilding(s) && CheckNames(s, expiredNames)),
                    It.Is<string>(s => s == etagLock)),
                    Times.Once);
        }

        private static bool Rebuilding(string data)
        {
            return JsonConvert.DeserializeObject<CacheValue>(data).Rebuilding;
        }

        private static bool CheckNames(string data, DeviceTwinName expiredNames)
        {
            var cacheValue = JsonConvert.DeserializeObject<CacheValue>(data);
            return cacheValue.Tags.SetEquals(expiredNames.Tags)
                   && cacheValue.Reported.SetEquals(expiredNames.ReportedProperties);
        }
    }
}
