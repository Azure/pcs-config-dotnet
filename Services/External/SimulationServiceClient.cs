// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class SimulationServiceClient : ISimulationServiceClient
    {
        private readonly IHttpClient httpClient;
        private readonly string serviceUri;

        public SimulationServiceClient(IHttpClient httpClient, IServicesConfig config)
        {
            this.httpClient = httpClient;
            this.serviceUri = config.DeviceSimulationApiUrl;
        }

        public async Task<HashSet<string>> GetDevicePropertyNamesAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/DeviceModels");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            var response = await this.httpClient.GetAsync(request);

            var content = JsonConvert.DeserializeObject<DeviceModelListApiModel>(response.Content);
            return content.GetPropNames();
        }
    }
}
