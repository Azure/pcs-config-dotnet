// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public class Cache : ICache
    {
        private readonly IStorageAdapterClient storageClient;
        private readonly IIothubManagerServiceClient iotHubClient;
        private readonly ISimulationServiceClient simulationClient;
        private readonly ILogger log;
        private readonly long cacheTTL;
        private readonly long rebuildTimeout;
        internal const string CacheCollectionId = "cache";
        internal const string CacheKey = "twin";

        public Cache(IStorageAdapterClient storageClient, IIothubManagerServiceClient iotHubClient, ISimulationServiceClient simulationClient, IServicesConfig config, ILogger logger)
        {
            this.storageClient = storageClient;
            this.iotHubClient = iotHubClient;
            this.simulationClient = simulationClient;
            log = logger;
            cacheTTL = config.CacheTTL;
            rebuildTimeout = config.CacheRebuildTimeout;
        }

        public async Task<CacheModel> GetCacheAsync()
        {
            try
            {
                var response = await storageClient.GetAsync(CacheCollectionId, CacheKey);
                return JsonConvert.DeserializeObject<CacheModel>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                log.Info($"{CacheCollectionId}:{CacheKey} not found.", () => $"{this.GetType().FullName}.GetCacheAsync");
                return new CacheModel { Tags = new HashSet<string>(), Reported = new HashSet<string>() };
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
                    cache = await storageClient.GetAsync(CacheCollectionId, CacheKey);
                }
                catch (ResourceNotFoundException)
                {
                    log.Info($"{CacheCollectionId}:{CacheKey} not found.", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                }

                bool needBuild = NeedBuild(force, cache);
                if (!needBuild)
                {
                    return;
                }

                try
                {
                    var twinNamesTask = iotHubClient.GetDeviceTwinNamesAsync();
                    var reportedNamesTask = simulationClient.GetDevicePropertyNamesAsync();
                    Task.WaitAll(twinNamesTask, reportedNamesTask);
                    twinNames = twinNamesTask.Result;
                    reportedNames = reportedNamesTask.Result;
                    reportedNames.UnionWith(twinNames.ReportedProperties);
                }
                catch (Exception)
                {
                    log.Info($"retry{retry}: IothubManagerService and SimulationService  are not both ready,wait 10 seconds ", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                    if (retry-- < 1)
                    {
                        return;
                    }
                    await Task.Delay(10000);
                    continue;
                }

                if (cache != null)
                {
                    CacheModel model = JsonConvert.DeserializeObject<CacheModel>(cache.Data);
                    model.Rebuilding = true;
                    var response = await storageClient.UpdateAsync(CacheCollectionId, CacheKey, JsonConvert.SerializeObject(model), cache.ETag);
                    etag = response.ETag;
                }
                else
                {
                    var response = await storageClient.UpdateAsync(CacheCollectionId, CacheKey, JsonConvert.SerializeObject(new CacheModel { Rebuilding = true }), null);
                    etag = response.ETag;
                }

                var value = JsonConvert.SerializeObject(new CacheModel
                {
                    Rebuilding = false,
                    Tags = twinNames.Tags,
                    Reported = reportedNames
                });

                try
                {
                    await storageClient.UpdateAsync(CacheCollectionId, CacheKey, value, etag);
                    return;
                }
                catch (ConflictingResourceException)
                {
                    log.Info("rebuild Conflicted ", () => $"{this.GetType().FullName}.RebuildCacheAsync");
                    continue;
                }
            }
        }

        public async Task<CacheModel> SetCacheAsync(CacheModel cache)
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
                    model = await storageClient.GetAsync(CacheCollectionId, CacheKey);
                }
                catch (ResourceNotFoundException)
                {
                    log.Info($"{CacheCollectionId}:{CacheKey} not found.", () => $"{this.GetType().FullName}.SetCacheAsync");
                }

                if (model != null)
                {
                    CacheModel cacheServer;

                    try
                    {
                        cacheServer = JsonConvert.DeserializeObject<CacheModel>(model.Data);
                    }
                    catch
                    {
                        cacheServer = new CacheModel();
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
                    var response = await storageClient.UpdateAsync(CacheCollectionId, CacheKey, value, etag);
                    return JsonConvert.DeserializeObject<CacheModel>(response.Data);
                }
                catch (ConflictingResourceException)
                {
                    log.Info("SetCache Conflicted ", () => $"{this.GetType().FullName}.RebuildCacheAsync");
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
                bool rebuilding = JsonConvert.DeserializeObject<CacheModel>(twin.Data).Rebuilding;
                DateTimeOffset timstamp = DateTimeOffset.Parse(twin.Metadata["$modified"]);
                needBuild = needBuild || !rebuilding && timstamp.AddSeconds(cacheTTL) < DateTimeOffset.UtcNow;
                needBuild = needBuild || rebuilding && timstamp.AddSeconds(rebuildTimeout) < DateTimeOffset.UtcNow;
            }
            return needBuild;
        }
    }
}
