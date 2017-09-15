// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Services.Test
{
    public class IothubManagerServiceClientTest
    {
        private const string MockServiceUri = @"http://hubManager";
        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly IothubManagerServiceClient client;

        public IothubManagerServiceClientTest()
        {
            mockHttpClient = new Mock<IHttpClient>();
            client = new IothubManagerServiceClient(
                mockHttpClient.Object,
                new ServicesConfig
                {
                    HubManagerApiUrl = MockServiceUri
                },
                new Logger("UnitTest", LogLevel.Debug));
        }

        [Fact]
        public async Task GetDeviceTwinNamesAsyncTest()
        {
            string content = @"
                {
                  ""Items"": [
                    {
                      ""Properties"": {
                        ""Reported"": {
                          ""device1a"": ""a"",
                          ""device1b"": { ""b"": ""b"" },
                          ""c"": ""c""
                        }
                      },
                      ""Tags"": {
                        ""device1e"": ""e"",
                        ""device1f"": { ""f"": ""f"" },
                        ""g"": ""g""
                      }
                  
                    },
                    {
                      ""Properties"": {
                        ""Reported"": {
                          ""device2a"": ""a"",
                          ""device2b"": { ""b"": ""b"" },
                          ""c"": ""c""
                        }
                      },
                      ""Tags"": {
                        ""device2e"": ""e"",
                        ""device2f"": { ""f"": ""f"" },
                        ""g"": ""g""
                      }
                  
                    }
                  ],
                  ""$metadata"": {
                    ""$type"": ""DeviceList;1"",
                    ""$uri"": ""/v1/devices""
                  }
                 }
                ";
            
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = content
            };

            mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await client.GetDeviceTwinNamesAsync();
            var tagNames = string.Join(",", result.Tags.OrderBy(m => m));
            var reportNames = string.Join(",", result.ReportedProperties.OrderBy(m => m));

            Assert.Equal(tagNames, string.Join(",", (new string[] { "device1e", "device1f.f", "device2e", "device2f.f", "g" }).OrderBy(m => m)));
            Assert.Equal(reportNames, string.Join(",", (new string[] { "device1a", "device1b.b", "c", "device2a", "device2b.b" }).OrderBy(m => m)));
        }
    }
}
