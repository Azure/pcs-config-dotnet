// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public class Seed : ISeed
    {
        private const string SeedCollectionId = "solution-settings";
        private const string MutexKey = "seedMutex";
        private const string CompletedFlagKey = "seedCompleted";
        private readonly TimeSpan mutexTimeout = TimeSpan.FromMinutes(5);

        private readonly IServicesConfig config;
        private readonly IStorageMutex mutex;
        private readonly IStorage storage;
        private readonly IStorageAdapterClient storageClient;
        private readonly IDeviceSimulationClient simulationClient;
        private readonly IDeviceTelemetryClient telemetryClient;
        private readonly ILogger logger;

        public Seed(
            IServicesConfig config,
            IStorageMutex mutex,
            IStorage storage,
            IStorageAdapterClient storageClient,
            IDeviceSimulationClient simulationClient,
            IDeviceTelemetryClient telemetryClient,
            ILogger logger)
        {
            this.config = config;
            this.mutex = mutex;
            this.storage = storage;
            this.storageClient = storageClient;
            this.simulationClient = simulationClient;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
        }

        public async Task TrySeedAsync()
        {
            if (!await mutex.Enter(SeedCollectionId, MutexKey, mutexTimeout))
            {
                logger.Info("Seed skipped (conflict)", () => { });
                return;
            }

            if (await CheckCompletedFlagAsync())
            {
                logger.Info("Seed skipped (completed)", () => { });
                return;
            }

            logger.Info("Seed begin", () => { });
            await SeedAsync(config.SeedTemplate);
            logger.Info("Seed end", () => { });

            await SetCompletedFlagAsync();

            await mutex.Leave(SeedCollectionId, MutexKey);
        }

        private async Task<bool> CheckCompletedFlagAsync()
        {
            try
            {
                await storageClient.GetAsync(SeedCollectionId, CompletedFlagKey);
                return true;
            }
            catch (ResourceNotFoundException)
            {
                return false;
            }
        }

        private async Task SetCompletedFlagAsync()
        {
            await storageClient.UpdateAsync(SeedCollectionId, CompletedFlagKey, "true", "*");
        }

        private async Task SeedAsync(string template)
        {
            string content;

            var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var file = Path.Combine(root, "Data", $"{template}.json");
            if (!File.Exists(file))
            {
                // ToDo: Check if `template` is a valid URL and try to load the content

                throw new ResourceNotFoundException($"Template {template} does not exist");
            }
            else
            {
                content = File.ReadAllText(file);
            }

            await SeedSingleTemplateAsync(content);
        }

        private async Task SeedSingleTemplateAsync(string content)
        {
            TemplateModel template;

            try
            {
                template = JsonConvert.DeserializeObject<TemplateModel>(content);
            }
            catch (Exception ex)
            {
                throw new InvalidInputException("Failed to parse template", ex);
            }

            if (template.Groups.Select(g => g.Id).Distinct().Count() != template.Groups.Count())
            {
                logger.Warn("Found duplicated group ID", () => new { template.Groups });
            }

            if (template.Rules.Select(r => r.Id).Distinct().Count() != template.Rules.Count())
            {
                logger.Warn("Found duplicated rule ID", () => new { template.Rules });
            }

            var groupIds = new HashSet<string>(template.Groups.Select(g => g.Id));
            var rulesWithInvalidGroupId = template.Rules.Where(r => !groupIds.Contains(r.GroupId));
            if (rulesWithInvalidGroupId.Any())
            {
                logger.Warn("Invalid group ID found in rules", () => new { rulesWithInvalidGroupId });
            }

            foreach (var group in template.Groups)
            {
                try
                {
                    await storage.UpdateDeviceGroupAsync(group.Id, group, "*");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to seed default group {group.DisplayName}", () => new { group, ex.Message });
                }
            }

            foreach (var rule in template.Rules)
            {
                try
                {
                    await telemetryClient.UpdateRuleAsync(rule, "*");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to seed default rule {rule.Description}", () => new { rule, ex.Message });
                }
            }

            try
            {
                var simulationModel = await this.simulationClient.GetSimulationAsync();

                if (simulationModel != null)
                {
                    logger.Info("Skip seed simulation since there is already one simuation", () => new { simulationModel });
                }
                else
                {
                    simulationModel = new SimulationApiModel
                    {
                        Id = "1",
                        Etag = "*"
                    };

                    simulationModel.DeviceModels = template.DeviceModels.ToList();
                    await simulationClient.UpdateSimulation(simulationModel);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to seed default simulation", () => new { ex.Message });
            }
        }
    }
}