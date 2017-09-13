// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public class SeedStatusManager : ISeedStatusManager
    {
        private readonly IStorageAdapterClient storageClient;

        private const string SolutionCollectionId = "solution-settings";
        private const string SeedStatusKey = "seedStatus";

        private const string SeedStatusSeeding = "seeding";
        private const string SeedStatusCompleted = "completed";

        public SeedStatusManager(IStorageAdapterClient storageClient)
        {
            this.storageClient = storageClient;
        }

        public async Task<bool> TryBeginSeedAsync()
        {
            while (true)
            {
                string etag;

                try
                {
                    var model = await storageClient.GetAsync(SolutionCollectionId, SeedStatusKey);
                    etag = model.ETag;

                    // Directly return `false` if seeding was done, or any other instance is seeding
                    if (model.Data == SeedStatusSeeding || model.Data == SeedStatusCompleted)
                    {
                        return false;
                    }
                }
                catch (ResourceNotFoundException)
                {
                    // No seeding flag
                    etag = "*";
                }

                try
                {
                    await storageClient.UpdateAsync(SolutionCollectionId, SeedStatusKey, SeedStatusSeeding, etag);

                    // Successfully updated the status to `seeding`, return `true`
                    return true;
                }
                catch (ConflictingResourceException)
                {
                    // Etag does not match. Restart the whole process
                }
            }
        }

        public async Task EndSeedAsync()
        {
            await storageClient.UpdateAsync(SolutionCollectionId, SeedStatusKey, SeedStatusCompleted, "*");
        }
    }
}