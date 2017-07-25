﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.Path), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class SolutionController : Controller
    {
        private readonly IStorage storage;

        public SolutionController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet("solution-settings")]
        public async Task<object> GetSettingsAsync()
        {
            return await storage.GetSettingsAsync();
        }

        [HttpPut("solution-settings")]
        public async Task<object> SetSettingsAsync([FromBody]object settings)
        {
            return await storage.SetSettingsAsync(settings);
        }

        [HttpGet("solution-logo")]
        public async Task GetLogoAsync()
        {
            var model = await storage.GetLogoAsync();
            SetImageResponse(model);
        }

        [HttpPut("solution-logo")]
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
