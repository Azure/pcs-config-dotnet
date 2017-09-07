// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageAdapterApiUrl { get; }

        string HubManagerApiUrl { get; }

        string SimulationApiUrl { get; }

        long CacheTTL { get; }

        long CacheRebuildTimeout { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageAdapterApiUrl { get; set; }

        public string HubManagerApiUrl { get; set; }

        public string SimulationApiUrl { get; set; }

        public long CacheTTL { get; set; }

        public long CacheRebuildTimeout { get; set; }

    }
}
