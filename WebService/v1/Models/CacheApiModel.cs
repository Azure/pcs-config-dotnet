using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class CacheApiModel
    {
        [JsonProperty("Tags")]
        public HashSet<string> Tags { get; set; }

        [JsonProperty("Reported")]
        public HashSet<string> Reported { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public CacheApiModel()
        {
           
        }

        public CacheApiModel(CacheModel model)
        {
            Tags = model.Tags;
            Reported = model.Reported;
            Metadata = new Dictionary<string, string>
            {
                { "$type", $"Cache;{Version.Number}" },
                { "$url", $"/{Version.Path}/caches" }
            };
        }

        public CacheModel ToServiceModel()
        {
            return new CacheModel
            {
                Tags = Tags,
                Reported = Reported
            };
        }
    }
}
