// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageAdapterApiUrl { get; }
        string DeviceSimulationApiUrl { get; }
        string DeviceTelemetryApiUrl { get; }
        string HubManagerApiUrl { get; }
        string SeedTemplate { get; }
        long CacheTTL { get; }
        long CacheRebuildTimeout { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageAdapterApiUrl { get; set; }
        public string DeviceSimulationApiUrl { get; set; }
        public string DeviceTelemetryApiUrl { get; set; }
        public string HubManagerApiUrl { get; set; }
        public string SeedTemplate { get; set; }
        public long CacheTTL { get; set; }
        public long CacheRebuildTimeout { get; set; }
    }
}
