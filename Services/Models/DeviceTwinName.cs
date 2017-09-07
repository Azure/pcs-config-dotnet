using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class DeviceTwinName
    {
        public HashSet<string> Tags { get; set; }

        public HashSet<string> ReportedProperties { get; set; }
    }
}
