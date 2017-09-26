// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class SolutionSettingsController : Controller
    {
        private readonly IStorage storage;

        public SolutionSettingsController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet("solution-settings/theme")]
        public async Task<object> GetThemeAsync()
        {
            return await this.storage.GetThemeAsync();
        }

        [HttpPut("solution-settings/theme")]
        public async Task<object> SetThemeAsync([FromBody] object theme)
        {
            return await this.storage.SetThemeAsync(theme);
        }

        [HttpGet("solution-settings/logo")]
        public async Task GetLogoAsync()
        {
            var model = await this.storage.GetLogoAsync();
            this.SetImageResponse(model);
        }

        [HttpPut("solution-settings/logo")]
        public async Task SetLogoAsync()
        {
            var bytes = new byte[this.Request.Body.Length];
            this.Request.Body.Read(bytes, 0, (int) this.Request.Body.Length);

            var model = new Logo
            {
                Image = Convert.ToBase64String(bytes),
                Type = this.Request.Headers["content-type"]
            };

            var response = await this.storage.SetLogoAsync(model);
            this.SetImageResponse(response);
        }

        private void SetImageResponse(Logo model)
        {
            var bytes = Convert.FromBase64String(model.Image);
            this.Response.Headers.Add("content-type", model.Type);
            this.Response.Body.Write(bytes, 0, bytes.Length);
        }
    }
}
