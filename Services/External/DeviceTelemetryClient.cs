// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IDeviceTelemetryClient
    {
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

            serviceUri = config.DeviceTelemetryApiUrl;
        }

        public async Task UpdateRuleAsync(RuleApiModel rule, string etag)
        {
            rule.ETag = etag;

            await httpClient.PutAsync($"{serviceUri}/rules/{rule.Id}", $"Rule {rule.Id}", rule);
        }
    }
}
