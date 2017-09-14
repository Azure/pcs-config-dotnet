using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class TemplateModel
    {
        [JsonProperty("Groups")]
        public IEnumerable<DeviceGroupServiceModel> Groups;

        [JsonProperty("Rules")]
        public IEnumerable<RuleApiModel> Rules;

        [JsonProperty("DeviceModels")]
        public IEnumerable<DeviceModelRef> DeviceModels;
    }
}
