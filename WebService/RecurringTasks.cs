// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService
{
    public interface IRecurringTasks
    {
        void Run();
    }

    public class RecurringTasks : IRecurringTasks
    {
        // When seed data creation fails, retry in few seconds
        // using a simple backoff logic
        private const int SEED_RETRY_INIT_SECS = 1;
        private const int SEED_RETRY_MAX_SECS = 8;

        // Allow some time for seed data to be created, shouldn't take too long though
        private const int SEED_TIMEOUT_SECS = 30;

        // When cache initialization fails, retry in few seconds
        private const int CACHE_INIT_RETRY_SECS = 10;

        // After the cache is initialized, update it every few minutes
        private const int CACHE_UPDATE_SECS = 300;

        // When generating the cache, allow some time to finish, at least one minute
        private const int CACHE_TIMEOUT_SECS = 90;

        private readonly ISeed seed;
        private readonly ICache cache;
        private readonly ILogger log;

        public RecurringTasks(
            ISeed seed,
            ICache cache,
            ILogger logger)
        {
            this.seed = seed;
            this.cache = cache;
            this.log = logger;
        }

        public void Run()
        {
            this.SetupSeedData();
            this.BuildCache();
            this.ScheduleCacheUpdate();
        }

        private void SetupSeedData(object context = null)
        {
            var pauseSecs = SEED_RETRY_INIT_SECS;
            while (true)
            {
                try
                {
                    this.log.Info("Creating seed data...", () => { });
                    this.seed.TrySeedAsync().Wait(SEED_TIMEOUT_SECS * 1000);
                    this.log.Info("Seed data created", () => { });
                    return;
                }
                catch (Exception e)
                {
                    this.log.Warn("Seed data setup failed, will retry in few seconds", () => new { pauseSecs, e });
                }

                this.log.Warn("Pausing thread before retrying seed data", () => new { pauseSecs });
                Thread.Sleep(pauseSecs * 1000);

                // Increase the pause, up to a maximum
                pauseSecs = Math.Min(pauseSecs + 1, SEED_RETRY_MAX_SECS);
            }
        }

        private void BuildCache()
        {
            while (true)
            {
                try
                {
                    this.log.Info("Creating cache...", () => { });
                    this.cache.TryRebuildCacheAsync().Wait(CACHE_TIMEOUT_SECS * 1000);
                    this.log.Info("Cache created", () => { });
                    return;
                }
                catch (Exception e)
                {
                    this.log.Warn("Cache creation failed, will retry in few seconds", () => new { CACHE_INIT_RETRY_SECS, e });
                }

                this.log.Warn("Pausing thread before retrying cache creation", () => new { CACHE_INIT_RETRY_SECS });
                Thread.Sleep(CACHE_INIT_RETRY_SECS * 1000);
            }
        }

        private void ScheduleCacheUpdate()
        {
            try
            {
                this.log.Info("Scheduling a cache update", () => new { CACHE_UPDATE_SECS });
                var unused = new Timer(
                    this.UpdateCache,
                    null,
                    1000 * CACHE_UPDATE_SECS,
                    Timeout.Infinite);
                this.log.Info("Cache update scheduled", () => new { CACHE_UPDATE_SECS });
            }
            catch (Exception e)
            {
                this.log.Error("Cache update scheduling failed", () => new { e });
            }
        }

        private void UpdateCache(object context = null)
        {
            try
            {
                this.log.Info("Updating cache...", () => { });
                this.cache.TryRebuildCacheAsync().Wait(CACHE_TIMEOUT_SECS * 1000);
                this.log.Info("Cache updated", () => { });
            }
            catch (Exception e)
            {
                this.log.Warn("Cache update failed, will retry later", () => new { CACHE_UPDATE_SECS, e });
            }

            this.ScheduleCacheUpdate();
        }
    }
}
