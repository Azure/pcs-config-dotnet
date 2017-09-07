using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class SimulationServiceClient : ISimulationServiceClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly string serviceUri;

        public SimulationServiceClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.serviceUri = config.SimulationApiUrl;
        }

        public async Task<HashSet<string>> GetDevicePropertyNamesAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{serviceUri}/DeviceModels");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }
            var response = await httpClient.GetAsync(request);
            DeviceModelListApiModel content = JsonConvert.DeserializeObject<DeviceModelListApiModel>(response.Content);
            return content.GetPropNames();
        }
    }
}
