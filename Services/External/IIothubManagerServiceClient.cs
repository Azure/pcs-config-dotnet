// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IIothubManagerServiceClient
    {
        Task<DeviceTwinName> GetDeviceTwinNamesAsync();
    }
}
