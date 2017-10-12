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
    public class IothubManagerServiceClientTest
    {
        private const string MOCK_SERVICE_URI = @"http://hubManager";
        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly IothubManagerServiceClient client;

        public IothubManagerServiceClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.client = new IothubManagerServiceClient(this.mockHttpClient.Object,
                new ServicesConfig
                {
                    HubManagerApiUrl = MOCK_SERVICE_URI
                });
        }

        [Fact]
        public async Task GetDeviceTwinNamesAsyncTest()
        {
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new
                {
                    Items = new object[] {
                        new {
                            Properties = new {
                                Reported = new {
                                    device1a = "a",
                                    device1b = new { b = "b" },
                                    c = "c"
                                }
                            },
                            Tags = new {
                                device1e = "e",
                                device1f = new { f = "f" },
                                g = "g"
                            }
                        },
                        new {
                            Properties = new {
                                Reported = new {
                                    device2a = "a",
                                    device2b = new { b = "b" },
                                    c = "c"
                                }
                            },
                            Tags = new {
                                device2e = "e",
                                device2f = new { f = "f" },
                                g = "g"
                            }
                        }
                    }
                })
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await this.client.GetDeviceTwinNamesAsync();

            Assert.True(result.Tags.SetEquals(new[] { "device1e", "device1f.f", "device2e", "device2f.f", "g" }));
            Assert.True(result.ReportedProperties.SetEquals(new[] { "device1a", "device1b.b", "c", "device2a", "device2b.b" }));
        }
    }
}
