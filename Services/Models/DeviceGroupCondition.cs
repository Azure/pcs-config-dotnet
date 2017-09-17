// Copyright (c) Microsoft. All rights reserved.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class DeviceGroupCondition
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Operator")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OperatorType Operator { get; set; }

        [JsonProperty("Value")]
        public Object Value { get; set; }
    }

    public enum OperatorType
    {
        EQ, // =
        NE, // !=
        LT, // <
        GT, // >
        LE, // <=
        GE, // >=
        IN // IN
    }
}
