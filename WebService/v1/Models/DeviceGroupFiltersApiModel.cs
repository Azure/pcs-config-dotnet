// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class DeviceGroupFiltersApiModel
    {
        [JsonProperty("Tags")]
        public HashSet<string> Tags { get; set; }

        [JsonProperty("Reported")]
        public HashSet<string> Reported { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public DeviceGroupFiltersApiModel()
        {
        }

        public DeviceGroupFiltersApiModel(CacheValue model)
        {
            this.Tags = model.Tags;
            this.Reported = model.Reported;
            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DeviceGroupFilters;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceGroupFilters" }
            };
        }

        public CacheValue ToServiceModel()
        {
            return new CacheValue
            {
                Tags = this.Tags,
                Reported = this.Reported
            };
        }
    }
}
