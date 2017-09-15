// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

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

        public DeviceGroupFiltersApiModel(CacheModel model)
        {
            Tags = model.Tags;
            Reported = model.Reported;
            Metadata = new Dictionary<string, string>
            {
                { "$type", $"DeviceGroupFilters;{Version.Number}" },
                { "$url", $"/{Version.Path}/deviceGroupFilters" }
            };
        }

        public CacheModel ToServiceModel()
        {
            return new CacheModel
            {
                Tags = Tags,
                Reported = Reported
            };
        }
    }
}
