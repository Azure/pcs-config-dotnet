// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    interface IAuthClient
    {
        Task<ProtocolListApiModel> GetAllAsync();
    }
}
