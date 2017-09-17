﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.Config.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.Config.WebService.v1.Models
{
    public class DeviceGroupApiModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Conditions")]
        public IEnumerable<DeviceGroupCondition> Conditions { get; set; }

        [JsonProperty("ETag")]
        public string ETag { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public DeviceGroupApiModel()
        {
        }

        public DeviceGroupApiModel(DeviceGroup model)
        {
            this.Id = model.Id;
            this.DisplayName = model.DisplayName;
            this.Conditions = model.Conditions;
            this.ETag = model.ETag;

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DeviceGroup;{Version.Number}" },
                { "$url", $"/{Version.Path}/devicegroups/{model.Id}" }
            };
        }

        public DeviceGroup ToServiceModel()
        {
            return new DeviceGroup
            {
                DisplayName = this.DisplayName,
                Conditions = this.Conditions
            };
        }
    }
}
