using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class DeviceModelListApiModel
    {
        [JsonProperty(PropertyName = "Items")]
        public List<DeviceModelApiModel> Items { get; set; }

        public HashSet<string> GetPropNames()
        {
            if (Items?.Count > 0)
            {
                HashSet<string> set = new HashSet<string>();
                Items.ForEach(m =>
                {
                    foreach (var item in m.Properties)
                    {
                        PreparePropNames(set, item.Value, item.Key);
                    }
                });
                return set;
            }
            return null;
        }
        private void PreparePropNames(HashSet<string> set, object obj, string prefix)
        {
            if (obj is JValue)
            {
                set.Add(prefix);
                return;
            }
            double outValue;
            if (obj is bool || obj is string || double.TryParse(obj.ToString(), out outValue))
            {
                set.Add(prefix);
                return;
            }
            foreach (var item in (obj as JToken).Values())
            {
                string path = item.Path;
                PreparePropNames(set, item, $"{prefix}.{(path.Contains(".") ? path.Substring(path.LastIndexOf('.') + 1) : path)}");
            }
        }
    }

    public class DeviceModelApiModel
    {
        [JsonProperty(PropertyName = "Properties")]
        public IDictionary<string, object> Properties { get; set; }
    }
}
