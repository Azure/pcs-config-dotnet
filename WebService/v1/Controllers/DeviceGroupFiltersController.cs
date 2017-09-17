﻿// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Config.Services;
using Microsoft.Azure.IoTSolutions.Config.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.Config.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.Config.WebService.v1.Controllers
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
            return new DeviceGroupFiltersApiModel(await this.cache.GetCacheAsync());
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAsync([FromBody] DeviceGroupFiltersApiModel input)
        {
            await this.cache.SetCacheAsync(input.ToServiceModel());
            return this.Ok();
        }

        [HttpPost("rebuild")]
        [DepressedFilter]
        public async Task<ActionResult> RebuildAsync()
        {
            await this.cache.RebuildCacheAsync(true);
            return this.Ok();
        }
    }
}
