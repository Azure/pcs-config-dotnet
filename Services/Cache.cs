// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface ICache
    {
        Task<CacheValue> GetCacheAsync();

        Task<CacheValue> SetCacheAsync(CacheValue cache);

        Task RebuildCacheAsync(bool force = false);
    }

    public class Cache : ICache
    {
        private readonly IStorageAdapterClient storageClient;
        private readonly IIothubManagerServiceClient iotHubClient;
        private readonly ISimulationServiceClient simulationClient;
        private readonly ILogger log;
        private readonly long cacheTtl;
        private readonly long rebuildTimeout;
        internal const string CACHE_COLLECTION_ID = "cache";
        internal const string CACHE_KEY = "twin";

        public Cache(IStorageAdapterClient storageClient,
            IIothubManagerServiceClient iotHubClient,
            ISimulationServiceClient simulationClient,
            IServicesConfig config,
            ILogger logger)
        {
            this.storageClient = storageClient;
            this.iotHubClient = iotHubClient;
            this.simulationClient = simulationClient;
            this.log = logger;
            this.cacheTtl = config.CacheTTL;
            this.rebuildTimeout = config.CacheRebuildTimeout;
        }

        public async Task<CacheValue> GetCacheAsync()
        {
            try
            {
                var response = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                return JsonConvert.DeserializeObject<CacheValue>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                this.log.Info($"{CACHE_COLLECTION_ID}:{CACHE_KEY} not found.", () => $"{this.GetType().FullName}.GetCacheAsync");
                return new CacheValue { Tags = new HashSet<string>(), Reported = new HashSet<string>() };
            }
        }

        public async Task RebuildCacheAsync(bool force = false)
        {
            int retry = 5;
            while (true)
            {
                HashSet<string> reportedNames = null;
                DeviceTwinName twinNames = null;
                ValueApiModel cache = null;
                string etag = null;

                try
                {
                    cache = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                }
                catch (ResourceNotFoundException)
                {
                    this.log.Info($"{CACHE_COLLECTION_ID}:{CACHE_KEY} not found.", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                }

                bool needBuild = this.NeedBuild(force, cache);
                if (!needBuild)
                {
                    return;
                }

                try
                {
                    var twinNamesTask = this.iotHubClient.GetDeviceTwinNamesAsync();
                    var reportedNamesTask = this.simulationClient.GetDevicePropertyNamesAsync();
                    Task.WaitAll(twinNamesTask, reportedNamesTask);
                    twinNames = twinNamesTask.Result;
                    reportedNames = reportedNamesTask.Result;
                    reportedNames.UnionWith(twinNames.ReportedProperties);
                }
                catch (Exception)
                {
                    this.log.Info($"retry{retry}: IothubManagerService and SimulationService  are not both ready,wait 10 seconds ", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                    if (retry-- < 1)
                    {
                        return;
                    }
                    await Task.Delay(10000);
                    continue;
                }

                if (cache != null)
                {
                    CacheValue model = JsonConvert.DeserializeObject<CacheValue>(cache.Data);
                    model.Rebuilding = true;
                    var response = await this.storageClient.UpdateAsync(CACHE_COLLECTION_ID, CACHE_KEY, JsonConvert.SerializeObject(model), cache.ETag);
                    etag = response.ETag;
                }
                else
                {
                    var response = await this.storageClient.UpdateAsync(CACHE_COLLECTION_ID, CACHE_KEY, JsonConvert.SerializeObject(new CacheValue { Rebuilding = true }), null);
                    etag = response.ETag;
                }

                var value = JsonConvert.SerializeObject(new CacheValue
                {
                    Rebuilding = false,
                    Tags = twinNames.Tags,
                    Reported = reportedNames
                });

                try
                {
                    await this.storageClient.UpdateAsync(CACHE_COLLECTION_ID, CACHE_KEY, value, etag);
                    return;
                }
                catch (ConflictingResourceException)
                {
                    this.log.Info("rebuild Conflicted ", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                    continue;
                }
            }
        }

        public async Task<CacheValue> SetCacheAsync(CacheValue cache)
        {
            // To simplify code, use empty set to replace null set
            cache.Tags = cache.Tags ?? new HashSet<string>();
            cache.Reported = cache.Reported ?? new HashSet<string>();

            string etag = null;
            while (true)
            {
                ValueApiModel model = null;
                try
                {
                    model = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                }
                catch (ResourceNotFoundException)
                {
                    this.log.Info($"{CACHE_COLLECTION_ID}:{CACHE_KEY} not found.", () => $"{this.GetType().FullName}.SetCacheAsync");
                }

                if (model != null)
                {
                    CacheValue cacheServer;

                    try
                    {
                        cacheServer = JsonConvert.DeserializeObject<CacheValue>(model.Data);
                    }
                    catch
                    {
                        cacheServer = new CacheValue();
                    }
                    cacheServer.Tags = cacheServer.Tags ?? new HashSet<string>();
                    cacheServer.Reported = cacheServer.Reported ?? new HashSet<string>();

                    cache.Tags.UnionWith(cacheServer.Tags);
                    cache.Reported.UnionWith(cacheServer.Reported);
                    etag = model.ETag;
                    if (cache.Tags.Count == cacheServer.Tags.Count && cache.Reported.Count == cacheServer.Reported.Count)
                    {
                        return cache;
                    }
                }

                var value = JsonConvert.SerializeObject(cache);
                try
                {
                    var response = await this.storageClient.UpdateAsync(CACHE_COLLECTION_ID, CACHE_KEY, value, etag);
                    return JsonConvert.DeserializeObject<CacheValue>(response.Data);
                }
                catch (ConflictingResourceException)
                {
                    this.log.Info("SetCache Conflicted ", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                    continue;
                }
            }
        }

        private bool NeedBuild(bool force, ValueApiModel twin)
        {
            bool needBuild = false;
            // validate timestamp
            if (force || twin == null)
            {
                needBuild = true;
            }
            else
            {
                bool rebuilding = JsonConvert.DeserializeObject<CacheValue>(twin.Data).Rebuilding;
                DateTimeOffset timstamp = DateTimeOffset.Parse(twin.Metadata["$modified"]);
                needBuild = needBuild || !rebuilding && timstamp.AddSeconds(this.cacheTtl) < DateTimeOffset.UtcNow;
                needBuild = needBuild || rebuilding && timstamp.AddSeconds(this.rebuildTimeout) < DateTimeOffset.UtcNow;
            }
            return needBuild;
        }
    }
}
