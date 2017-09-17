// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.Config.Services.Models;

namespace Microsoft.Azure.IoTSolutions.Config.Services.External
{
    public interface IIothubManagerServiceClient
    {
        Task<DeviceTwinName> GetDeviceTwinNamesAsync();
    }
}
