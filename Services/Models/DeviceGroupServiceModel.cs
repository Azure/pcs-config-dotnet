// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class DeviceGroupServiceModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public IList<DeviceGroupConditionModel> Conditions { get; set; }
        public string ETag { get; set; }
    }
}
