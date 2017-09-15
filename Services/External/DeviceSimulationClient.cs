﻿// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task<SimulationApiModel> GetSimulationAsync();
        Task UpdateSimulation(SimulationApiModel model);
    }

    public class DeviceSimulationClient : IDeviceSimulationClient
    {
        private const int SimulationId = 1;
        private readonly IHttpClientWrapper httpClient;
        private readonly string serviceUri;

        public DeviceSimulationClient(
            IHttpClientWrapper httpClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            serviceUri = config.DeviceSimulationApiUrl;
        }

        public async Task<SimulationApiModel> GetSimulationAsync()
        {
            return await httpClient.GetAsync<SimulationApiModel>($"{serviceUri}/simulations/{SimulationId}", $"Simulation {SimulationId}", true);
        }

        public async Task UpdateSimulation(SimulationApiModel model)
        {
            await httpClient.PutAsync($"{serviceUri}/simulations/{SimulationId}", $"Simulation {SimulationId}", model);
        }
    }
}
