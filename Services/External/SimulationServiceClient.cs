﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.Config.Services.Runtime;
using Microsoft.Azure.IoTSolutions.Config.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Config.Services.Http;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.Config.Services.External
{
    public class SimulationServiceClient : ISimulationServiceClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;

        public SimulationServiceClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
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
            DeviceModelListApiModel content = JsonConvert.DeserializeObject<DeviceModelListApiModel>(response.Content);
            return content.GetPropNames();
        }
    }
}
