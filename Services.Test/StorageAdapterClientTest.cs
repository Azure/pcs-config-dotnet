// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class StorageAdapterClientTest
    {
        private const string MockServiceUri = @"http://mockstorageadapter";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly StorageAdapterClient client;
        private readonly Random rand;

        public StorageAdapterClientTest()
        {
            mockHttpClient = new Mock<IHttpClient>();
            client = new StorageAdapterClient(
                mockHttpClient.Object,
                new ServicesConfig
                {
                    StorageAdapterApiUrl = MockServiceUri
                },
                new Logger("UnitTest", LogLevel.Debug));
            rand = new Random();
        }

        [Fact]
        public async Task GetAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new ValueApiModel
                {
                    Key = key,
                    Data = data,
                    ETag = etag
                })
            };

            mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await client.GetAsync(collectionId, key);

            mockHttpClient
                .Verify(x => x.GetAsync(
                    It.Is<IHttpRequest>(r => r.Check($"{MockServiceUri}/collections/{collectionId}/values/{key}"))),
                    Times.Once);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
        }

        [Fact]
        public async Task GetAsyncNotFoundTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccessStatusCode = false
            };

            mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await client.GetAsync(collectionId, key));
        }

        [Fact]
        public async Task GetAllAsyncTest()
        {
            var collectionId = rand.NextString();
            var models = new[]
            {
                new ValueApiModel
                {
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString()
                },
                new ValueApiModel
                {
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString()
                },
                new ValueApiModel
                {
                    Key = rand.NextString(),
                    Data = rand.NextString(),
                    ETag = rand.NextString()
                }
            };

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new ValueListApiModel { Items = models })
            };

            mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await client.GetAllAsync(collectionId);

            mockHttpClient
                .Verify(x => x.GetAsync(
                    It.Is<IHttpRequest>(r => r.Check($"{MockServiceUri}/collections/{collectionId}/values"))),
                    Times.Once);

            Assert.Equal(result.Items.Count(), models.Length);
            foreach (var item in result.Items)
            {
                var model = models.Single(m => m.Key == item.Key);
                Assert.Equal(model.Data, item.Data);
                Assert.Equal(model.ETag, item.ETag);
            }
        }

        [Fact]
        public async Task CreateAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new ValueApiModel
                {
                    Key = key,
                    Data = data,
                    ETag = etag
                })
            };

            mockHttpClient
                .Setup(x => x.PostAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await client.CreateAsync(collectionId, data);

            mockHttpClient
                .Verify(x => x.PostAsync(
                    It.Is<IHttpRequest>(r => r.Check<ValueApiModel>($"{MockServiceUri}/collections/{collectionId}/values", m => m.Data == data))),
                    Times.Once);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etag);
        }

        [Fact]
        public async Task UpdateAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etagOld = rand.NextString();
            var etagNew = rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(new ValueApiModel
                {
                    Key = key,
                    Data = data,
                    ETag = etagNew
                })
            };

            mockHttpClient
                .Setup(x => x.PutAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await client.UpdateAsync(collectionId, key, data, etagOld);

            mockHttpClient
                .Verify(x => x.PutAsync(
                    It.Is<IHttpRequest>(r => r.Check<ValueApiModel>($"{MockServiceUri}/collections/{collectionId}/values/{key}", m => m.Data == data && m.ETag == etagOld))),
                    Times.Once);

            Assert.Equal(result.Key, key);
            Assert.Equal(result.Data, data);
            Assert.Equal(result.ETag, etagNew);
        }

        [Fact]
        public async Task UpdateAsyncConflictTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();
            var data = rand.NextString();
            var etag = rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                IsSuccessStatusCode = false
            };

            mockHttpClient
                .Setup(x => x.PutAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<ConflictingResourceException>(async () =>
                await client.UpdateAsync(collectionId, key, data, etag));
        }

        [Fact]
        public async Task DeleteAsyncTest()
        {
            var collectionId = rand.NextString();
            var key = rand.NextString();

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };

            mockHttpClient
                .Setup(x => x.DeleteAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await client.DeleteAsync(collectionId, key);

            mockHttpClient
                .Verify(x => x.DeleteAsync(
                    It.Is<IHttpRequest>(r => r.Check($"{MockServiceUri}/collections/{collectionId}/values/{key}"))),
                    Times.Once);
        }
    }
}
