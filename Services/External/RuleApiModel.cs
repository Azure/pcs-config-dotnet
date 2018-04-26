// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class RuleApiModel
    {
        [JsonProperty(PropertyName = "ETag")]
        public string ETag { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "GroupId")]
        public string GroupId { get; set; }

        [JsonProperty(PropertyName = "Severity")]
        public string Severity { get; set; }

        [JsonProperty(PropertyName = "Conditions")]
        public List<ConditionApiModel> Conditions { get; set; }

        // Possible values -["Average", "Instant"]
        [JsonProperty(PropertyName = "Calculation")]
        public string Calculation { get; set; }

        // Possible values -["00:01:00", "00:05:00", "00:10:00"]
        [JsonProperty(PropertyName = "TimePeriod")]
        public TimeSpan TimePeriod { get; set; }
    }
}
