// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public class Storage : IStorage
    {
        private readonly ILogger logger;
        private readonly IStorageAdapterClient client;
        private static string SolutionCollectionId = "solution";
        private static string KeySettings = "settings";
        private static string KeyLogo = "logo";
        private static string DeviceGroupCollection = "deviceGroups";

        public Storage(ILogger logger, IStorageAdapterClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<object> GetSettingsAsync()
        {
            try
            {
                var response = await client.GetAsync(SolutionCollectionId, KeySettings);
                return JsonConvert.DeserializeObject(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return SettingsServiceModel.Default;
            }
        }

        public async Task<object> SetSettingsAsync(object settings)
        {
            var value = JsonConvert.SerializeObject(settings);
            var response = await client.UpdateAsync(SolutionCollectionId, KeySettings, value, "*");
            return JsonConvert.DeserializeObject(response.Data);
        }

        public async Task<LogoServiceModel> GetLogoAsync()
        {
            try
            {
                var response = await client.GetAsync(SolutionCollectionId, KeyLogo);
                return JsonConvert.DeserializeObject<LogoServiceModel>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return LogoServiceModel.Default;
            }
        }

        public async Task<LogoServiceModel> SetLogoAsync(LogoServiceModel model)
        {
            var value = JsonConvert.SerializeObject(model);
            var response = await client.UpdateAsync(SolutionCollectionId, KeyLogo, value, "*");
            return JsonConvert.DeserializeObject<LogoServiceModel>(response.Data);
        }

        public async Task<IEnumerable<DeviceGroupServiceModel>> GetAllDeviceGroupsAsync()
        {
            var response = await client.GetAllAsync(DeviceGroupCollection);
            return response.Items.Select(CreateGroupServiceModel);
        }

        public async Task<DeviceGroupServiceModel> GetDeviceGroup(string id)
        {
            var response = await client.GetAsync(DeviceGroupCollection, id);
            return CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroupServiceModel> CreateDeviceGroup(DeviceGroupServiceModel input)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await client.CreateAsync(DeviceGroupCollection, value);
            return CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroupServiceModel> UpdateDeviceGroup(string id, DeviceGroupServiceModel input, string etag)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await client.UpdateAsync(DeviceGroupCollection, id, value, etag);
            return CreateGroupServiceModel(response);
        }

        public async Task DeleteDeviceGroup(string id)
        {
            await client.DeleteAsync(DeviceGroupCollection, id);
        }

        private DeviceGroupServiceModel CreateGroupServiceModel(ValueApiModel input)
        {
            var output = JsonConvert.DeserializeObject<DeviceGroupServiceModel>(input.Data);
            output.Id = input.Key;
            output.ETag = input.ETag;
            return output;
        }
    }
}
