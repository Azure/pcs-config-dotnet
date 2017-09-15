// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public class Storage : IStorage
    {
        private readonly IStorageAdapterClient client;
        internal const string SolutionCollectionId = "solution-settings";
        internal const string ThemeKey = "theme";
        internal const string LogoKey = "logo";
        internal const string UserCollectionId = "user-settings";
        internal const string DeviceGroupCollectionId = "devicegroups";

        public Storage(IStorageAdapterClient client)
        {
            this.client = client;
        }

        public async Task<object> GetThemeAsync()
        {
            try
            {
                var response = await client.GetAsync(SolutionCollectionId, ThemeKey);
                return JsonConvert.DeserializeObject(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return ThemeServiceModel.Default;
            }
        }

        public async Task<object> SetThemeAsync(object theme)
        {
            var value = JsonConvert.SerializeObject(theme);
            var response = await client.UpdateAsync(SolutionCollectionId, ThemeKey, value, "*");
            return JsonConvert.DeserializeObject(response.Data);
        }

        public async Task<object> GetUserSetting(string id)
        {
            try
            {
                var response = await client.GetAsync(UserCollectionId, id);
                return JsonConvert.DeserializeObject(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return new object();
            }
        }

        public async Task<object> SetUserSetting(string id, object setting)
        {
            var value = JsonConvert.SerializeObject(setting);
            var response = await client.UpdateAsync(UserCollectionId, id, value, "*");
            return JsonConvert.DeserializeObject(response.Data);
        }

        public async Task<LogoServiceModel> GetLogoAsync()
        {
            try
            {
                var response = await client.GetAsync(SolutionCollectionId, LogoKey);
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
            var response = await client.UpdateAsync(SolutionCollectionId, LogoKey, value, "*");
            return JsonConvert.DeserializeObject<LogoServiceModel>(response.Data);
        }

        public async Task<IEnumerable<DeviceGroupServiceModel>> GetAllDeviceGroupsAsync()
        {
            var response = await client.GetAllAsync(DeviceGroupCollectionId);
            return response.Items.Select(CreateGroupServiceModel);
        }

        public async Task<DeviceGroupServiceModel> GetDeviceGroupAsync(string id)
        {
            var response = await client.GetAsync(DeviceGroupCollectionId, id);
            return CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroupServiceModel> CreateDeviceGroupAsync(DeviceGroupServiceModel input)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await client.CreateAsync(DeviceGroupCollectionId, value);
            return CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroupServiceModel> UpdateDeviceGroupAsync(string id, DeviceGroupServiceModel input, string etag)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await client.UpdateAsync(DeviceGroupCollectionId, id, value, etag);
            return CreateGroupServiceModel(response);
        }

        public async Task DeleteDeviceGroupAsync(string id)
        {
            await client.DeleteAsync(DeviceGroupCollectionId, id);
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