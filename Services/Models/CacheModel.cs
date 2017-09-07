using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class CacheModel
    {
        public bool Rebuilding { get; set; } = false;

        public HashSet<string> Tags { get; set; }

        public HashSet<string> Reported { get; set; }

    }
}
