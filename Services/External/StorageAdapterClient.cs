// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;
using HttpClient = Microsoft.Azure.IoTSolutions.UIConfig.Services.Http.HttpClient;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class StorageAdapterClient : IStorageAdapterClient
    {
        private readonly IServicesConfig config;
        private readonly HttpClient httpClient;

        public StorageAdapterClient(IServicesConfig config, ILogger logger)
        {
            this.config = config;
            this.httpClient = new HttpClient(logger);
        }

        public async Task<ValueApiModel> GetAsync(string collectionId, string key)
        {
            var request = CreateRequest($"collections/{collectionId}/values/{key}");
            var response = await httpClient.GetAsync(request);
            CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueListApiModel> GetAllAsync(string collectionId)
        {
            var request = CreateRequest($"collections/{collectionId}/values");
            var response = await httpClient.GetAsync(request);
            CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueListApiModel>(response.Content);
        }

        public async Task<ValueApiModel> CreateAsync(string collectionId, string value)
        {
            var request = CreateRequest($"collections/{collectionId}/values", new ValueApiModel
            {
                Data = value
            });
            var response = await httpClient.PostAsync(request);
            CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag)
        {
            var request = CreateRequest($"collections/{collectionId}/values/{key}", new ValueApiModel
            {
                Data = value,
                ETag = etag
            });
            var response = await httpClient.PutAsync(request);
            CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ValueApiModel>(response.Content);
        }

        public async Task DeleteAsync(string collectionId, string key)
        {
            var request = CreateRequest($"collections/{collectionId}/values/{key}");
            var response = await httpClient.DeleteAsync(request);
            CheckStatusCode(response, request);
        }

        private HttpRequest CreateRequest(string path, ValueApiModel content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{config.StorageAdapterApiUrl}/{path}");
            request.Options.AllowInsecureSSLServer = true;

            if (content != null)
            {
                request.SetContent(content);
            }

            return request;
        }

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException($"{response.Content}, request URL = {request.Uri}");

                case HttpStatusCode.Conflict:
                    throw new ConflictingResourceException($"{response.Content}, request URL = {request.Uri}");

                default:
                    throw new HttpRequestException($"Http request failed, status code = {response.StatusCode}, content = {response.Content}, request URL = {request.Uri}");
            }
        }
    }
}
