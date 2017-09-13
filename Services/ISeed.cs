// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface ISeed
    {
        Task TrySeedAsync();
    }
}
