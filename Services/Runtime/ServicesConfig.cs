// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageAdapterApiUrl { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageAdapterApiUrl { get; set; }
    }
}
