// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IDeviceTelemetryClient
    {
        Task<RuleApiModel> GetRuleAsync(string id);
        Task CreateRuleAsync(RuleApiModel rule);
        Task UpdateRuleAsync(RuleApiModel rule, string etag);
    }

    public class DeviceTelemetryClient : IDeviceTelemetryClient
    {
        private readonly IHttpClientWrapper httpClient;
        private readonly string serviceUri;

        public DeviceTelemetryClient(
            IHttpClientWrapper httpClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            this.serviceUri = config.TelemetryApiUrl;
        }

        public async Task<RuleApiModel> GetRuleAsync(string id)
        {
            return await this.httpClient.GetAsync<RuleApiModel>($"{this.serviceUri}/rules/{id}", "Get rule", true);
        }

        public async Task CreateRuleAsync(RuleApiModel rule)
        {
            await this.httpClient.PostAsync($"{this.serviceUri}/rules", $"Rule {rule.Id}", rule);
        }

        public async Task UpdateRuleAsync(RuleApiModel rule, string etag)
        {
            rule.ETag = etag;

            await this.httpClient.PutAsync($"{this.serviceUri}/rules/{rule.Id}", $"Rule {rule.Id}", rule);
        }
    }
}
