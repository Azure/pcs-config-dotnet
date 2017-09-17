// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class DeviceListApiModel
    {
        public List<DeviceRegistryApiModel> Items { get; set; }

        public DeviceTwinName GetDeviceTwinNames()
        {
            if (this.Items?.Count > 0)
            {
                var tagSet = new HashSet<string>();
                var reportedSet = new HashSet<string>();
                this.Items.ForEach(m =>
                {
                    foreach (var item in m.Tags)
                    {
                        this.PrepareTagNames(tagSet, item.Value, item.Key);
                    }
                    foreach (var item in m.Properties.Reported)
                    {
                        this.PrepareTagNames(reportedSet, item.Value, item.Key);
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
                this.PrepareTagNames(set, item, $"{prefix}.{(path.Contains(".") ? path.Substring(item.Path.LastIndexOf('.') + 1) : path)}");
            }
        }
    }
}
