// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IStorageMutex
    {
        Task<bool> Enter(string collectionId, string key, TimeSpan timeout);
        Task Leave(string collectionId, string key);
    }
}