// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public class Seed : ISeed
    {
        private readonly ISeedStatusManager seedStatusManager;
        private readonly IStorage storage;
        private readonly IDeviceSimulationClient simulationClient;
        private readonly IDeviceTelemetryClient telemetryClient;
        private readonly ILogger logger;

        public Seed(
            ISeedStatusManager seedStatusManager,
            IStorage storage,
            IDeviceSimulationClient simulationClient,
            IDeviceTelemetryClient telemetryClient,
            ILogger logger)
        {
            this.seedStatusManager = seedStatusManager;
            this.storage = storage;
            this.simulationClient = simulationClient;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
        }

        public async Task TrySeedAsync()
        {
            if (!await seedStatusManager.TryBeginSeedAsync())
            {
                logger.Info("Seeding skipped", () => { });
                return;
            }

            logger.Info("Seeding begin", () => { });
            await SeedAsync();
            logger.Info("Seeding end", () => { });

            await seedStatusManager.EndSeedAsync();
        }

        private async Task SeedAsync()
        {
            var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var di = new DirectoryInfo(Path.Combine(root, "Data"));
            foreach (var template in di.GetFiles("*.json"))
            {
                await SeedSingleTemplateAsync(template.FullName);
            }

            try
            {
                await simulationClient.CreateDefaultSimulationAsync();
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to seed default simulations", () => new { ex.Message });
            }
        }

        private async Task SeedSingleTemplateAsync(string file)
        {
            TemplateModel template;

            try
            {
                template = JsonConvert.DeserializeObject<TemplateModel>(File.ReadAllText(file));
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load template {file}", () => new { ex });
                return;
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
        }
    }
}