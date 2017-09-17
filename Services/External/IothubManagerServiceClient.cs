// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class IothubManagerServiceClient : IIothubManagerServiceClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;

        public IothubManagerServiceClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.HubManagerApiUrl;
        }

        public async Task<DeviceTwinName> GetDeviceTwinNamesAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/Devices");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }
            var response = await this.httpClient.GetAsync(request);
            DeviceListApiModel content = JsonConvert.DeserializeObject<DeviceListApiModel>(response.Content);
            return content.GetDeviceTwinNames();
        }
    }
}
