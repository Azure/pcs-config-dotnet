// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

        public async Task ConfigureApplication(IApplicationBuilder app)
        {
            var setup = false;
            ProtocolListApiModel protocols = null;

            try
            {
                protocols = await GetAllAsync();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load authentication protocols", () => new { ex.Message });
            }

            if (protocols != null)
            {
                foreach (var protocol in protocols.Items)
                {
                    switch (protocol.Type)
                    {
                        // Currently, only AAD Global is supported
                        case "oauth.AAD.Global":
                            setup = SetupGlobalAAD(app, protocol, protocols);
                            break;

                        default:
                            logger.Error("Unsupported authentication protocol", () => new { protocol });
                            break;
                    }
                }
            }

            if (!setup)
            {
                SetupNullValidator(app);
            }
        }

        private bool SetupGlobalAAD(IApplicationBuilder app, ProtocolApiModel protocol, ProtocolListApiModel protocols)
        {
            if (!protocol.Parameters.ContainsKey("tenantId") || !protocol.Parameters.ContainsKey("clientId"))
            {
                logger.Error("Missing tenantId/clientId, ignore protocol", () => new { protocol });
                return false;
            }

            var tenantId = protocol.Parameters["tenantId"];
            var clientId = protocol.Parameters["clientId"];
            var supportedSignatureAlgorithms = protocols.SupportedSignatureAlgorithms;

            var options = new JwtBearerOptions
            {
                Authority = $"https://login.microsoftonline.com/{tenantId}", //Required?
                Audience = clientId
            };

            if (supportedSignatureAlgorithms != null)
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new SecurityTokenSignatureAlgorithmValidator(supportedSignatureAlgorithms));
            }

            app.UseJwtBearerAuthentication(options);

            logger.Info("JwtBearer authentication setup successfully", () => new { tenantId, clientId, supportedSignatureAlgorithms });
            return true;
        }

        private void SetupNullValidator(IApplicationBuilder app)
        {
            var options = new JwtBearerOptions();
            options.SecurityTokenValidators.Clear();
            options.SecurityTokenValidators.Add(new SecurityTokenNullValidator());
            app.UseJwtBearerAuthentication(options);

            logger.Info("Null authentication setup. All call to authorized API will return `Unauthorized`", () => { });
        }
    }
}
