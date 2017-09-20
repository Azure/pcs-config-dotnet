// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }

        /// <summary>CORS whitelist, in form { "origins": [], "methods": [], "headers": [] }</summary>
        string CorsWhitelist { get; }

        /// <summary>Service layer configuration</summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "Config:";
        private const string PortKey = ApplicationKey + "webservice_port";
        private const string CorsWhitelistKey = ApplicationKey + "cors_whitelist";
        private const string SeedTemplateKey = ApplicationKey + "seedTemplate";
        private const string CacheTTLKey = ApplicationKey + "cache_TTL";
        private const string CacheRebuildTimeoutKey = ApplicationKey + "rebuild_timeout";
        private const string BingMapKeyKey = ApplicationKey + "bingmap_key";

        private const string StorageAdapterKey = "StorageAdapter:";
        private const string StorageAdapterUrlKey = StorageAdapterKey + "webservice_url";

        private const string DeviceSimulationKey = "DeviceSimulation:";
        private const string DeviceSimulationUrlKey = DeviceSimulationKey + "webservice_url";

        private const string TelemetryKey = "Telemetry:";
        private const string TelemetryUrlKey = TelemetryKey + "webservice_url";

        private const string HubManagerKey = "IothubManager:";
        private const string HubManagerUrlKey = HubManagerKey + "webservice_url";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>CORS whitelist, in form { "origins": [], "methods": [], "headers": [] }</summary>
        public string CorsWhitelist { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PortKey);
            this.CorsWhitelist = configData.GetString(CorsWhitelistKey);

            this.ServicesConfig = new ServicesConfig
            {
                StorageAdapterApiUrl = configData.GetString(StorageAdapterUrlKey),
                DeviceSimulationApiUrl = configData.GetString(DeviceSimulationUrlKey),
                TelemetryApiUrl = configData.GetString(TelemetryUrlKey),
                HubManagerApiUrl = configData.GetString(HubManagerUrlKey),
                SeedTemplate = configData.GetString(SeedTemplateKey),
                CacheTTL = configData.GetInt(CacheTTLKey),
                CacheRebuildTimeout = configData.GetInt(CacheRebuildTimeoutKey),
                BingMapKey = configData.GetString(BingMapKeyKey)
            };
        }
    }
}
