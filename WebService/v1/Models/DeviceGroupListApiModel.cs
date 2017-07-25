// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;


namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class DeviceGroupListApiModel
    {
        public IEnumerable<DeviceGroupApiModel> Items { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public DeviceGroupListApiModel(IEnumerable<DeviceGroupServiceModel> models)
        {
            Items = models.Select(m => new DeviceGroupApiModel(m));

            Metadata = new Dictionary<string, string>
            {
                { "$type", $"DeviceGroupList;{Version.Number}" },
                { "$url", $"/{Version.Path}/devicegroups" }
            };
        }
    }
}
