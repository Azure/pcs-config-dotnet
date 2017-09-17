// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class DeviceRegistryApiModel
    {
        [JsonProperty(PropertyName = "Properties")]
        public TwinPropertiesApiModel Properties { get; set; }

        [JsonProperty(PropertyName = "Tags")]
        public Dictionary<string, JToken> Tags { get; set; }
    }
}
