// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class IothubManagerServiceClient : IIothubManagerServiceClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly string serviceUri;

        public IothubManagerServiceClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            serviceUri = config.HubManagerApiUrl;
        }

        public async Task<DeviceTwinName> GetDeviceTwinNamesAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{serviceUri}/Devices");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }
            var response = await httpClient.GetAsync(request);
            DeviceListApiModel content = JsonConvert.DeserializeObject<DeviceListApiModel>(response.Content);
            return content.GetDeviceTwinNames();
        }
    }
}
