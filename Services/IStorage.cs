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
        Task<DeviceGroupServiceModel> GetDeviceGroupAsync(string id);
        Task<DeviceGroupServiceModel> CreateDeviceGroupAsync(DeviceGroupServiceModel input);
        Task<DeviceGroupServiceModel> UpdateDeviceGroupAsync(string id, DeviceGroupServiceModel input, string etag);
        Task DeleteDeviceGroupAsync(string id);
    }
}
