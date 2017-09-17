﻿// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.Config.Services.Runtime;
using Microsoft.Azure.IoTSolutions.Config.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Config.Services.External;
using Microsoft.Azure.IoTSolutions.Config.Services.Http;
using Moq;
using Xunit;

namespace Services.Test
{
    public class SimulationServiceClientTest
    {
        private const string MockServiceUri = @"http://SimulationManager";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly SimulationServiceClient client;

        public SimulationServiceClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.client = new SimulationServiceClient(this.mockHttpClient.Object,
                new ServicesConfig
                {
                    DeviceSimulationApiUrl = MockServiceUri
                },
                new Logger("UnitTest", LogLevel.Debug));
        }

        [Fact]
        public async Task GetDevicePropertyNamesAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = @"{
                              ""Items"": [
                                {
                                  ""key"": ""value"",
                                  ""Properties"": {
                                    ""Type"": ""Truck"",
                                    ""Location"": ""Field"",
                                    ""address"": {
                                      ""street"": ""ssss"",
                                      ""NO"": ""1111""
                                    },
                                    ""Latitude"": 47.445301,
                                    ""Longitude"": -122.296307
                                  }
                                },
                                {
                            
                                  ""Properties"": {
                                    ""Type1"": ""Truck"",
                                    ""Location"": ""Field"",
                                    ""address1"": {
                                      ""street"": ""ssss"",
                                      ""NO"": ""1111""
                                    },
                                    ""Latitude1"": 47.445301,
                                    ""Longitude"": -122.296307
                                  }
                                }
                              ]
                            }"
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = string.Join(",", (await this.client.GetDevicePropertyNamesAsync()).OrderBy(m => m));

            Assert.Equal(result, string.Join(",", (new string[] { "address.NO", "address.street", "address1.NO", "address1.street", "Type", "Location", "Latitude", "Longitude", "Type1", "Latitude1" }).OrderBy(m => m)));
        }
    }
}
