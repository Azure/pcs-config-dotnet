using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

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
