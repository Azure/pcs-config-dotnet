// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    public class ProtocolListApiModel
    {
        public IEnumerable<ProtocolApiModel> Items { get; set; }

        public IEnumerable<string> SupportedSignatureAlgorithms { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
