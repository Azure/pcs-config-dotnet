// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
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

        Task<bool> RebuildCacheAsync(bool force = false);
    }

    public class Cache : ICache
    {
        private readonly IStorageAdapterClient storageClient;
        private readonly IIothubManagerServiceClient iotHubClient;
        private readonly ISimulationServiceClient simulationClient;
        private readonly ILogger log;
        private readonly string cacheWhitelist;
        private readonly long cacheTtl;
        private readonly long rebuildTimeout;
        private readonly TimeSpan serviceQueryInterval = TimeSpan.FromSeconds(10);
        internal const string CACHE_COLLECTION_ID = "cache";
        internal const string CACHE_KEY = "twin";

        private const string WHITELIST_TAG_PREFIX = "tags.";
        private const string WHITELIST_REPORTED_PREFIX = "reported.";

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
            this.cacheWhitelist = config.CacheWhiteList;
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
                this.log.Info($"Cache get: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found", () => { });
                return new CacheValue { Tags = new HashSet<string>(), Reported = new HashSet<string>() };
            }
        }

        public async Task<bool> RebuildCacheAsync(bool force = false)
        {
            int retry = 5;
            while (true)
            {
                ValueApiModel cache = null;

                // Retrieve cache to check if rebuilding is necessary
                try
                {
                    cache = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                }
                catch (ResourceNotFoundException)
                {
                    this.log.Info($"Cache rebuilding: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found", () => { });
                }

                var needBuild = this.NeedBuild(force, cache);
                if (!needBuild)
                {
                    this.log.Warn("Cache rebuilding skipped", () => { });
                    return false;
                }

                // Build the cache content
                DeviceTwinName twinNames;
                try
                {
                    var twinNamesTask = this.GetValidNamesAsync();
                    var simulationNamesTask = this.simulationClient.GetDevicePropertyNamesAsync();
                    Task.WaitAll(twinNamesTask, simulationNamesTask);
                    twinNames = twinNamesTask.Result;
                    twinNames.ReportedProperties.UnionWith(simulationNamesTask.Result);
                }
                catch (Exception)
                {
                    if (retry-- < 1)
                    {
                        this.log.Warn("Cache rebuilding failed due to out of retry times", () => { });
                        return false;
                    }

                    this.log.Warn($"Service IoTHubManager and/or Simulation are not ready. Retry after {serviceQueryInterval}", () => { });
                    await Task.Delay(serviceQueryInterval);
                    continue;
                }

                // Lock the cache for writing
                ValueApiModel model;
                try
                {
                    var cacheValue = cache == null ? new CacheValue() : JsonConvert.DeserializeObject<CacheValue>(cache.Data);
                    cacheValue.Rebuilding = true;

                    model = await this.storageClient.UpdateAsync(CACHE_COLLECTION_ID, CACHE_KEY, JsonConvert.SerializeObject(cacheValue), cache?.ETag);
                }
                catch (ConflictingResourceException)
                {
                    this.log.Warn("Cache rebuilding failed due to conflict while writing lock. Retry soon", () => { });
                    continue;
                }

                // Write the cache to storage                
                try
                {
                    await this.storageClient.UpdateAsync(
                        CACHE_COLLECTION_ID,
                        CACHE_KEY,
                        JsonConvert.SerializeObject(new CacheValue
                        {
                            Rebuilding = false,
                            Tags = twinNames.Tags,
                            Reported = twinNames.ReportedProperties
                        }),
                        model.ETag);

                    this.log.Info("Cache rebuilding completed", () => { });
                    return true;
                }
                catch (ConflictingResourceException)
                {
                    this.log.Warn("Cache rebuilding failed due to conflict while writing value. Retry soon", () => { });
                }
            }
        }

        private async Task<DeviceTwinName> GetValidNamesAsync()
        {
            ParseWhitelist(this.cacheWhitelist, out var fullNameWhitelist, out var prefixWhitelist);

            var validNames = new DeviceTwinName
            {
                Tags = fullNameWhitelist.Tags,
                ReportedProperties = fullNameWhitelist.ReportedProperties
            };

            if (prefixWhitelist.Tags.Any() || prefixWhitelist.ReportedProperties.Any())
            {
                var allNames = await this.iotHubClient.GetDeviceTwinNamesAsync();

                validNames.Tags.UnionWith(
                    allNames.Tags
                        .Where(s => prefixWhitelist.Tags.Any(s.StartsWith)));

                validNames.ReportedProperties.UnionWith(
                    allNames.ReportedProperties
                        .Where(s => prefixWhitelist.ReportedProperties.Any(s.StartsWith)));
            }

            return validNames;
        }

        private static void ParseWhitelist(string whitelist, out DeviceTwinName fullNameWhitelist, out DeviceTwinName prefixWhitelist)
        {
            var whitelistItems = whitelist.Split(',').Select(s => s.Trim());

            var tags = whitelistItems
                .Where(s => s.StartsWith(WHITELIST_TAG_PREFIX, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WHITELIST_TAG_PREFIX.Length));

            var reported = whitelistItems
                .Where(s => s.StartsWith(WHITELIST_REPORTED_PREFIX, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WHITELIST_REPORTED_PREFIX.Length));

            var fixedTags = tags.Where(s => !s.EndsWith("*"));
            var fixedReported = reported.Where(s => !s.EndsWith("*"));

            var regexTags = tags.Where(s => s.EndsWith("*")).Select(s => s.Substring(0, s.Length - 1));
            var regexReported = reported.Where(s => s.EndsWith("*")).Select(s => s.Substring(0, s.Length - 1));

            fullNameWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(fixedTags),
                ReportedProperties = new HashSet<string>(fixedReported)
            };

            prefixWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(regexTags),
                ReportedProperties = new HashSet<string>(regexReported)
            };
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
                    this.log.Info($"Cache updating: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found", () => { });
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
                    this.log.Info("Cache updating: failed due to conflict. Retry soon", () => { });
                }
            }
        }

        private bool NeedBuild(bool force, ValueApiModel twin)
        {
            if (force || twin == null)
            {
                return true;
            }

            var rebuilding = JsonConvert.DeserializeObject<CacheValue>(twin.Data).Rebuilding;
            var timstamp = DateTimeOffset.Parse(twin.Metadata["$modified"]);

            return !rebuilding && timstamp.AddSeconds(this.cacheTtl) < DateTimeOffset.UtcNow
                || rebuilding && timstamp.AddSeconds(this.rebuildTimeout) < DateTimeOffset.UtcNow;
        }
    }
}
