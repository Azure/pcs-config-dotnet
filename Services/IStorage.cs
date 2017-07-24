// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IStorage
    {
        Task<object> GetSettingsAsync();
        Task<object> SetSettingsAsync(object settings);
        Task<LogoServiceModel> GetLogoAsync();
        Task<LogoServiceModel> SetLogoAsync(LogoServiceModel model);
        Task<IEnumerable<DeviceGroupServiceModel>> GetAllDeviceGroupsAsync();
        Task<DeviceGroupServiceModel> GetDeviceGroup(string id);
        Task<DeviceGroupServiceModel> CreateDeviceGroup(DeviceGroupServiceModel input);
        Task<DeviceGroupServiceModel> UpdateDeviceGroup(string id, DeviceGroupServiceModel input, string etag);
        Task DeleteDeviceGroup(string id);
    }
}
