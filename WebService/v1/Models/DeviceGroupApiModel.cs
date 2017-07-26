// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class DeviceGroupApiModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public object Conditions { get; set; }
        public string ETag { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public DeviceGroupApiModel()
        {
        }

        public DeviceGroupApiModel(DeviceGroupServiceModel model)
        {
            Id = model.Id;
            DisplayName = model.DisplayName;
            Conditions = model.Conditions;
            ETag = model.ETag;

            Metadata = new Dictionary<string, string>
            {
                { "$type", $"DeviceGroup;{Version.Number}" },
                { "$url", $"/{Version.Path}/devicegroups/{model.Id}" }
            };
        }

        public DeviceGroupServiceModel ToServiceModel()
        {
            return new DeviceGroupServiceModel
            {
                DisplayName = this.DisplayName,
                Conditions = this.Conditions
            };
        }
    }
}
