// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Services.Test
{
    public class SimulationServiceClientTest
    {
        private const string MOCK_SERVICE_URI = @"http://SimulationManager";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly SimulationServiceClient client;

        public SimulationServiceClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.client = new SimulationServiceClient(this.mockHttpClient.Object,
                new ServicesConfig
                {
                    DeviceSimulationApiUrl = MOCK_SERVICE_URI
                });
        }

        [Fact]
        public async Task GetDevicePropertyNamesAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new
                {
                    Items = new object[] {
                        new {
                            key = "value",
                            Properties = new {
                                @Type = "Truck",
                                Location = "Field",
                                address = new {
                                    street = "ssss",
                                    NO = "1111"
                                },
                                Latitude = 47.445301,
                                Longitude = -122.296307
                            },
                        },
                        new {
                            Properties = new {
                                Type1 = "Truck",
                                Location = "Field",
                                address1 = new {
                                    street = "ssss",
                                    NO = "1111"
                                },
                                Latitude1 = 47.445301,
                                Longitude = -122.296307
                            }
                        }
                    }
                })
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await this.client.GetDevicePropertyNamesAsync();

            Assert.True(result.SetEquals(new[] { "address.NO", "address.street", "address1.NO", "address1.street", "Type", "Location", "Latitude", "Longitude", "Type1", "Latitude1" }));
        }
    }
}
