// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.Path), TypeFilter(typeof(ExceptionsFilterAttribute))]
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
            return await storage.GetThemeAsync();
        }

        [HttpPut("solution-settings/theme")]
        public async Task<object> SetThemeAsync([FromBody]object theme)
        {
            return await storage.SetThemeAsync(theme);
        }

        [HttpGet("solution-settings/logo")]
        public async Task GetLogoAsync()
        {
            var model = await storage.GetLogoAsync();
            SetImageResponse(model);
        }

        [HttpPut("solution-settings/logo")]
        public async Task SetLogoAsync()
        {
            var bytes = new byte[Request.Body.Length];
            Request.Body.Read(bytes, 0, (int)Request.Body.Length);

            var model = new LogoServiceModel
            {
                Image = Convert.ToBase64String(bytes),
                Type = Request.Headers["content-type"]
            };

            var response = await storage.SetLogoAsync(model);
            SetImageResponse(response);
        }

        private void SetImageResponse(LogoServiceModel model)
        {
            var bytes = Convert.FromBase64String(model.Image);
            Response.Headers.Add("content-type", model.Type);
            Response.Body.Write(bytes, 0, bytes.Length);
        }
    }
}
