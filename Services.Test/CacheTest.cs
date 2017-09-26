// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Newtonsoft.Json;
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
            this.cacheModel = JsonConvert.SerializeObject(new CacheValue
            {
                Rebuilding = false,
                Tags = new HashSet<string> { "c", "a", "y", "z" },
                Reported = new HashSet<string> { "1", "9", "2", "3" }
            });
            this.mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            this.mockIothubManagerClient = new Mock<IIothubManagerServiceClient>();
            this.mockSimulationClient = new Mock<ISimulationServiceClient>();
            this.log = new Mock<ILogger>();
            this.log.Setup(m => m.Info(It.IsAny<string>(), It.IsAny<Action>())).Callback(() => { });
            this.log.Setup(m => m.Info(It.IsAny<string>(), It.IsAny<Func<object>>())).Callback(() => { });
            this.log.Setup(m => m.Debug(It.IsAny<string>(), It.IsAny<Action>())).Callback(() => { });
            this.log.Setup(m => m.Debug(It.IsAny<string>(), It.IsAny<Func<object>>())).Callback(() => { });
            this.log.Setup(m => m.Error(It.IsAny<string>(), It.IsAny<Action>())).Callback(() => { });
            this.log.Setup(m => m.Error(It.IsAny<string>(), It.IsAny<Func<object>>())).Callback(() => { });
            this.config = new Mock<IServicesConfig>();
            this.config.SetupGet(m => m.CacheTTL).Returns(3600);
            this.config.SetupGet(m => m.CacheRebuildTimeout).Returns(20);
            this.cache = new Cache(this.mockStorageAdapterClient.Object, this.mockIothubManagerClient.Object, this.mockSimulationClient.Object, this.config.Object, this.log.Object);
        }

        [Fact]
        public async Task GetCacheAsyncTestAsync()
        {
            this.mockStorageAdapterClient.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ValueApiModel { Data = this.cacheModel }));
            var result = await this.cache.GetCacheAsync();
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
            this.mockStorageAdapterClient.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ValueApiModel { Data = this.cacheModel }));
            CacheValue model = new CacheValue { Tags = new HashSet<string> { "c", "a", "y", "z", "@", "#" }, Reported = new HashSet<string> { "1", "9", "2", "3", "12", "11" }, Rebuilding = false };
            this.mockStorageAdapterClient.Setup(m => m.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(model) }));
            var result = await this.cache.SetCacheAsync(new CacheValue { Tags = new HashSet<string> { "a", "y", "z", "@", "#" }, Reported = new HashSet<string> { "9", "2", "3", "11", "12" } });
            Assert.Equal(string.Join(",", (new string[] { "c", "a", "y", "z", "@", "#" }).OrderBy(m => m)),
                string.Join(",", result.Tags.OrderBy(m => m))
            );
            Assert.Equal(string.Join(",", (new string[] { "1", "9", "2", "3", "12", "11" }).OrderBy(m => m)),
                string.Join(",", result.Reported.OrderBy(m => m))
            );
        }
    }
}
