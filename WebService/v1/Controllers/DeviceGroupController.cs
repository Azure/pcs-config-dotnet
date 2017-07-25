// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.Path + "/devicegroups"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class DeviceGroupController : Controller
    {
        private readonly IStorage storage;

        public DeviceGroupController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet("")]
        public async Task<DeviceGroupListApiModel> GetAllAsync()
        {
            return new DeviceGroupListApiModel(await storage.GetAllDeviceGroupsAsync());
        }

        [HttpGet("{id}")]
        public async Task<DeviceGroupApiModel> GetAsync(string id)
        {
            return new DeviceGroupApiModel(await storage.GetDeviceGroupAsync(id));
        }

        [HttpPost]
        public async Task<DeviceGroupApiModel> CreateAsync([FromBody]DeviceGroupApiModel input)
        {
            var model = new DeviceGroupServiceModel
            {
                DisplayName = input.DisplayName,
                Conditions = input.Conditions
            };

            return new DeviceGroupApiModel(await storage.CreateDeviceGroupAsync(model));
        }

        [HttpPut("{id}")]
        public async Task<DeviceGroupApiModel> UpdateAsync(string id, [FromBody]DeviceGroupApiModel input)
        {
            var model = new DeviceGroupServiceModel
            {
                DisplayName = input.DisplayName,
                Conditions = input.Conditions
            };

            return new DeviceGroupApiModel(await storage.UpdateDeviceGroupAsync(id, model, input.ETag));
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await storage.DeleteDeviceGroupAsync(id);
        }
    }
}
