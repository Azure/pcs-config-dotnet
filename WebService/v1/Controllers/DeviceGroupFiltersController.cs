// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.Path + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class DeviceGroupFiltersController : Controller
    {
        private readonly ICache cache;

        public DeviceGroupFiltersController(ICache cache)
        {
            this.cache = cache;
        }

        [HttpGet]
        public async Task<DeviceGroupFiltersApiModel> GetAsync()
        {
            return new DeviceGroupFiltersApiModel(await cache.GetCacheAsync());
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAsync([FromBody] DeviceGroupFiltersApiModel input)
        {
            await cache.SetCacheAsync(input.ToServiceModel());
            return Ok();
        }

        [HttpPost("rebuild")]
        [DepressedFilter]
        public async Task<ActionResult> RebuildAsync()
        {
            await cache.RebuildCacheAsync(true);
            return Ok();
        }
    }
}
