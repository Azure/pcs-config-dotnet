// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Services.Test
{
    public class CacheTest
    {
        private readonly Mock<IStorageAdapterClient> mockStorageAdapterClient;
        private readonly Mock<IIothubManagerServiceClient> mockIothubManagerClient;
        private readonly Mock<ISimulationServiceClient> mockSimulationClient;
        private readonly Mock<IServicesConfig> config;
        private readonly Mock<ILogger> log;
        private readonly Cache cache;
        private readonly string cacheModel = null;

        public CacheTest()
        {
            cacheModel = JsonConvert.SerializeObject(new CacheModel
            {
                Rebuilding = false,
                Tags = new HashSet<string> { "c", "a", "y", "z" },
                Reported = new HashSet<string> { "1", "9", "2", "3" }
            });
            mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            mockIothubManagerClient = new Mock<IIothubManagerServiceClient>();
            mockSimulationClient = new Mock<ISimulationServiceClient>();
            log = new Mock<ILogger>();
            log.Setup(m => m.Info(It.IsAny<string>(), It.IsAny<Action>())).Callback(()=>{ });
            log.Setup(m => m.Info(It.IsAny<string>(), It.IsAny<Func<object>>())).Callback(() => { });
            log.Setup(m => m.Debug(It.IsAny<string>(), It.IsAny<Action>())).Callback(() => { });
            log.Setup(m => m.Debug(It.IsAny<string>(), It.IsAny<Func<object>>())).Callback(() => { });
            log.Setup(m => m.Error(It.IsAny<string>(), It.IsAny<Action>())).Callback(() => { });
            log.Setup(m => m.Error(It.IsAny<string>(), It.IsAny<Func<object>>())).Callback(() => { });
            config = new Mock<IServicesConfig>();
            config.SetupGet(m => m.CacheTTL).Returns(3600);
            config.SetupGet(m => m.CacheRebuildTimeout).Returns(20);
            cache = new Cache(mockStorageAdapterClient.Object, mockIothubManagerClient.Object, mockSimulationClient.Object, config.Object, log.Object);
        }

        [Fact]
        public async Task GetCacheAsyncTestAsync()
        {
            mockStorageAdapterClient.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ValueApiModel { Data = cacheModel }));
            var result = await cache.GetCacheAsync();
            Assert.Equal(string.Join(",", (new string[] { "c", "a", "y", "z" }).OrderBy(m => m)),
                string.Join(",", result.Tags.OrderBy(m => m))
                );
            Assert.Equal(string.Join(",", (new string[] { "1", "9", "2", "3" }).OrderBy(m => m)),
                string.Join(",", result.Reported.OrderBy(m => m))
                );
        }

        [Fact]
        public async Task SetCacheAsyncTest()
        {
            mockStorageAdapterClient.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ValueApiModel { Data = cacheModel }));
            CacheModel model = new CacheModel { Tags = new HashSet<string> { "c", "a", "y", "z", "@", "#" }, Reported = new HashSet<string> { "1", "9", "2", "3", "12", "11" }, Rebuilding = false };
            mockStorageAdapterClient.Setup(m => m.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(model) }));
            var result = await cache.SetCacheAsync(new CacheModel { Tags = new HashSet<string> { "a", "y", "z", "@", "#" }, Reported = new HashSet<string> { "9", "2", "3", "11", "12" } });
            Assert.Equal(string.Join(",", (new string[] { "c", "a", "y", "z", "@", "#" }).OrderBy(m => m)),
                string.Join(",", result.Tags.OrderBy(m => m))
                );
            Assert.Equal(string.Join(",", (new string[] { "1", "9", "2", "3", "12", "11" }).OrderBy(m => m)),
                string.Join(",", result.Reported.OrderBy(m => m))
                );
        }
    }
}
