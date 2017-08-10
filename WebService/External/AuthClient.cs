// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    public class AuthClient : IAuthClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly string serviceUri;

        public AuthClient(IHttpClient httpClient, IConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.serviceUri = config.AuthApiUrl;
        }

        public async Task<ProtocolListApiModel> GetAllAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{serviceUri}/protocols");
            request.Options.AllowInsecureSSLServer = true;

            var response = await httpClient.GetAsync(request);
            CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<ProtocolListApiModel>(response.Content);
        }

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            logger.Info($"Auth returns {response.StatusCode} for request {request.Uri}", () => new
            {
                request.Uri,
                response.StatusCode,
                response.Content
            });

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
