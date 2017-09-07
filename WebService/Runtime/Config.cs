// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
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
        private const string ApplicationKey = "UIConfig:";
        private const string PortKey = ApplicationKey + "webservice_port";
        private const string CorsWhitelistKey = ApplicationKey + "cors_whitelist";
        private const string CacheTTLKey = ApplicationKey + "cache_TTL";
        private const string CacheRebuildTimeoutKey = ApplicationKey + "rebuild_timeout";

        private const string StorageAdapterKey = "StorageAdapter:";
        private const string StorageAdapterUrlKey = StorageAdapterKey + "webservice_url";

        private const string HubManagerKey = "IothubManagerService:";
        private const string HubManagerUrlKey = HubManagerKey + "webservice_url";

        private const string SimulationKey = "SimulationService:";
        private const string SimulationUrlKey = SimulationKey + "webservice_url";

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
                HubManagerApiUrl = configData.GetString(HubManagerUrlKey),
                SimulationApiUrl = configData.GetString(SimulationUrlKey),
                CacheTTL = configData.GetInt(CacheTTLKey),
                CacheRebuildTimeout = configData.GetInt(CacheRebuildTimeoutKey)
            };
        }

        private static string MapRelativePath(string path)
        {
            if (path.StartsWith(".")) return AppContext.BaseDirectory + Path.DirectorySeparatorChar + path;
            return path;
        }
    }
}
