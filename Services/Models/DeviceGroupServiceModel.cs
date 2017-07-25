// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class DeviceGroupServiceModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public object Conditions { get; set; }
        public string ETag { get; set; }
    }
}
