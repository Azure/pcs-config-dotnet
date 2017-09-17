// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Config.Services;
using Microsoft.Azure.IoTSolutions.Config.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.Config.WebService.v1.Controllers
{
    [Route(Version.Path + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class SeedController : Controller
    {
        private readonly ISeed seed;

        public SeedController(ISeed seed)
        {
            this.seed = seed;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            await this.seed.TrySeedAsync();
        }
    }
}
