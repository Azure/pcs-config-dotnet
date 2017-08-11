// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    interface IAuthClient
    {
        Task<ProtocolListApiModel> GetAllAsync();

        Task ConfigureApplication(IApplicationBuilder app);
    }
}
