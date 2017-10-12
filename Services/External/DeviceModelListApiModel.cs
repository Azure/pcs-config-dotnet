// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class DeviceModelListApiModel
    {
        [JsonProperty(PropertyName = "Items")]
        public List<DeviceModelApiModel> Items { get; set; }

        public HashSet<string> GetPropNames()
        {
            if (this.Items?.Count > 0)
            {
                var set = new HashSet<string>();
                this.Items.ForEach(m =>
                {
                    foreach (var item in m.Properties)
                    {
                        this.PreparePropNames(set, item.Value, item.Key);
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

            if (obj is bool || obj is string || double.TryParse(obj.ToString(), out _))
            {
                set.Add(prefix);
                return;
            }

            foreach (var item in (obj as JToken).Values())
            {
                var path = item.Path;
                this.PreparePropNames(set, item, $"{prefix}.{(path.Contains(".") ? path.Substring(path.LastIndexOf('.') + 1) : path)}");
            }
        }
    }

    public class DeviceModelApiModel
    {
        [JsonProperty(PropertyName = "Properties")]
        public IDictionary<string, object> Properties { get; set; }
    }
}
