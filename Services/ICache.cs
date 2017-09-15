// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface ICache
    {
        Task<CacheModel> GetCacheAsync();

        Task<CacheModel> SetCacheAsync(CacheModel cache);

        Task RebuildCacheAsync(bool force = false);
    }
}
