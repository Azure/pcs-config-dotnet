﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IStorage
    {
        Task<object> GetThemeAsync();
        Task<object> SetThemeAsync(object theme);
        Task<object> GetUserSetting(string id);
        Task<object> SetUserSetting(string id, object setting);
        Task<Logo> GetLogoAsync();
        Task<Logo> SetLogoAsync(Logo model);
        Task<IEnumerable<DeviceGroup>> GetAllDeviceGroupsAsync();
        Task<DeviceGroup> GetDeviceGroupAsync(string id);
        Task<DeviceGroup> CreateDeviceGroupAsync(DeviceGroup input);
        Task<DeviceGroup> UpdateDeviceGroupAsync(string id, DeviceGroup input, string etag);
        Task DeleteDeviceGroupAsync(string id);
    }

    public class Storage : IStorage
    {
        private readonly IStorageAdapterClient client;
        private readonly IServicesConfig config;

        internal const string SolutionCollectionId = "solution-settings";
        internal const string ThemeKey = "theme";
        internal const string LogoKey = "logo";
        internal const string UserCollectionId = "user-settings";
        internal const string DeviceGroupCollectionId = "devicegroups";
        private const string BingMapKeyKey = "BingMapKey";

        public Storage(
            IStorageAdapterClient client,
            IServicesConfig config)
        {
            this.client = client;
            this.config = config;
        }

        public async Task<object> GetThemeAsync()
        {
            string data;

            try
            {
                var response = await this.client.GetAsync(SolutionCollectionId, ThemeKey);
                data = response.Data;
            }
            catch (ResourceNotFoundException)
            {
                data = JsonConvert.SerializeObject(Theme.Default);
            }

            var themeOut = JsonConvert.DeserializeObject(data) as JToken ?? new JObject();
            AppendBingMapKey(themeOut);
            return themeOut;
        }

        public async Task<object> SetThemeAsync(object themeIn)
        {
            var value = JsonConvert.SerializeObject(themeIn);
            var response = await this.client.UpdateAsync(SolutionCollectionId, ThemeKey, value, "*");
            var themeOut = JsonConvert.DeserializeObject(response.Data) as JToken ?? new JObject();
            AppendBingMapKey(themeOut);
            return themeOut;
        }

        private void AppendBingMapKey(JToken theme)
        {
            if (theme[BingMapKeyKey] == null)
            {
                theme[BingMapKeyKey] = config.BingMapKey;
            }
        }

        public async Task<object> GetUserSetting(string id)
        {
            try
            {
                var response = await this.client.GetAsync(UserCollectionId, id);
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
            var response = await this.client.UpdateAsync(UserCollectionId, id, value, "*");
            return JsonConvert.DeserializeObject(response.Data);
        }

        public async Task<Logo> GetLogoAsync()
        {
            try
            {
                var response = await this.client.GetAsync(SolutionCollectionId, LogoKey);
                return JsonConvert.DeserializeObject<Logo>(response.Data);
            }
            catch (ResourceNotFoundException)
            {
                return Logo.Default;
            }
        }

        public async Task<Logo> SetLogoAsync(Logo model)
        {
            var value = JsonConvert.SerializeObject(model);
            var response = await this.client.UpdateAsync(SolutionCollectionId, LogoKey, value, "*");
            return JsonConvert.DeserializeObject<Logo>(response.Data);
        }

        public async Task<IEnumerable<DeviceGroup>> GetAllDeviceGroupsAsync()
        {
            var response = await this.client.GetAllAsync(DeviceGroupCollectionId);
            return response.Items.Select(this.CreateGroupServiceModel);
        }

        public async Task<DeviceGroup> GetDeviceGroupAsync(string id)
        {
            var response = await this.client.GetAsync(DeviceGroupCollectionId, id);
            return this.CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroup> CreateDeviceGroupAsync(DeviceGroup input)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await this.client.CreateAsync(DeviceGroupCollectionId, value);
            return this.CreateGroupServiceModel(response);
        }

        public async Task<DeviceGroup> UpdateDeviceGroupAsync(string id, DeviceGroup input, string etag)
        {
            var value = JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var response = await this.client.UpdateAsync(DeviceGroupCollectionId, id, value, etag);
            return this.CreateGroupServiceModel(response);
        }

        public async Task DeleteDeviceGroupAsync(string id)
        {
            await this.client.DeleteAsync(DeviceGroupCollectionId, id);
        }

        private DeviceGroup CreateGroupServiceModel(ValueApiModel input)
        {
            var output = JsonConvert.DeserializeObject<DeviceGroup>(input.Data);
            output.Id = input.Key;
            output.ETag = input.ETag;
            return output;
        }
    }
}
