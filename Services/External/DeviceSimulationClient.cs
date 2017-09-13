// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task CreateDefaultSimulationAsync();
    }

    public class DeviceSimulationClient : IDeviceSimulationClient
    {
        private readonly IHttpClientWrapper httpClient;
        private readonly string serviceUri;

        public DeviceSimulationClient(
            IHttpClientWrapper httpClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            serviceUri = config.DeviceSimulationApiUrl;
        }

        public async Task CreateDefaultSimulationAsync()
        {
            await httpClient.PostAsync($"{serviceUri}/simulations?template=default", "DefaultSimulation");
        }
    }
}
