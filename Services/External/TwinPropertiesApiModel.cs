// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class TwinPropertiesApiModel
    {
        [JsonProperty(PropertyName = "Reported")]
        public Dictionary<string, JToken> Reported { get; set; }

        [JsonProperty(PropertyName = "Desired")]
        public Dictionary<string, JToken> Desired { get; set; }
    }
}
