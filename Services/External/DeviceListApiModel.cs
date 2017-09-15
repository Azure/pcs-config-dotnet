using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class DeviceListApiModel
    {
        public List<DeviceRegistryApiModel> Items { get; set; }

        public DeviceTwinName GetDeviceTwinNames()
        {
            if (Items?.Count > 0)
            {
                HashSet<string> tagSet = new HashSet<string>();
                HashSet<string> reportedSet = new HashSet<string>();
                Items.ForEach(m =>
                {
                    foreach (var item in m.Tags)
                    {
                        PrepareTagNames(tagSet, item.Value, item.Key);
                    }
                    foreach (var item in m.Properties.Reported)
                    {
                        PrepareTagNames(reportedSet, item.Value, item.Key);
                    }
                });
                return new DeviceTwinName { Tags = tagSet, ReportedProperties = reportedSet };
            }
            return null;
        }
        private void PrepareTagNames(HashSet<string> set, JToken jToken, string prefix)
        {
            if (jToken is JValue)
            {
                set.Add(prefix);
                return;
            }
            foreach (var item in jToken.Values())
            {
                string path = item.Path;
                PrepareTagNames(set, item, $"{prefix}.{(path.Contains(".") ? path.Substring(item.Path.LastIndexOf('.') + 1) : path)}");
            }
        }
    }
}
