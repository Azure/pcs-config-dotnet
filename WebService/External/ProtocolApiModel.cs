// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    public class ProtocolApiModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
