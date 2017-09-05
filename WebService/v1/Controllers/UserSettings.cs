﻿// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Authorize]
    [Route(Version.Path), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UserSettingsController : Controller
    {
        private readonly IStorage storage;

        public UserSettingsController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet("user-settings/{id}")]
        public async Task<object> GetUserSettingAsync(string id)
        {
            return await storage.GetUserSetting(id);
        }

        [HttpPut("user-settings/{id}")]
        public async Task<object> SetUserSettingAsync(string id, [FromBody]object setting)
        {
            return await storage.SetUserSetting(id, setting);
        }
    }
}
