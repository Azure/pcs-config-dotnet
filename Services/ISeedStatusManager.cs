// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface ISeedStatusManager
    {
        Task<bool> TryBeginSeedAsync();
        Task EndSeedAsync();
    }
}