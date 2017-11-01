﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers
{
    public interface IStorageMutex
    {
        Task<bool> EnterAsync(string collectionId, string key, TimeSpan timeout);
        Task LeaveAsync(string collectionId, string key);
    }

    public class StorageMutex : IStorageMutex
    {
        private const string LAST_MODIFIED_KEY = "$modified";
        private readonly IStorageAdapterClient storageClient;

        public StorageMutex(IStorageAdapterClient storageClient)
        {
            this.storageClient = storageClient;
        }

        public async Task<bool> EnterAsync(string collectionId, string key, TimeSpan timeout)
        {
            string etag = null;

            while (true)
            {
                try
                {
                    var model = await this.storageClient.GetAsync(collectionId, key);
                    etag = model.ETag;

                    // Mutex was captured by some other instance, return `false` except the state was not updated for a long time
                    // The motivation of timeout check is to recovery from stale state due to instance crash
                    if (Convert.ToBoolean(model.Data))
                    {
                        if (model.Metadata.ContainsKey(LAST_MODIFIED_KEY) && DateTimeOffset.TryParse(model.Metadata[LAST_MODIFIED_KEY], out var lastModified))
                        {
                            // Timestamp retrieved successfully, nothing to do
                        }
                        else
                        {
                            // Treat it as timeout if the timestamp could not be retrieved
                            lastModified = DateTimeOffset.MinValue;
                        }

                        if (DateTimeOffset.UtcNow < lastModified + timeout)
                        {
                            return false;
                        }
                    }
                }
                catch (ResourceNotFoundException)
                {
                    // Mutex is not initialized, treat it as released
                }

                try
                {
                    // In case there is no such a mutex, the `etag` will be null. It will cause
                    // a new mutex created, and the operation will be synchronized
                    await this.storageClient.UpdateAsync(collectionId, key, "true", etag);

                    // Successfully enter the mutex, return `true`
                    return true;
                }
                catch (ConflictingResourceException)
                {
                    // Etag does not match. Restart the whole process
                }
            }
        }

        public async Task LeaveAsync(string collectionId, string key)
        {
            await this.storageClient.UpdateAsync(collectionId, key, "false", "*");
        }
    }
}
