// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;

namespace WebService.Test.helpers
{
    internal sealed class MockHttpContext : IDisposable
    {
        private readonly HeaderDictionary requestHeaders = new HeaderDictionary();
        private readonly HeaderDictionary responseHeaders = new HeaderDictionary();
        private readonly MemoryStream requestBody = new MemoryStream();
        private readonly MemoryStream responseBody = new MemoryStream();
        private readonly Mock<HttpContext> mockContext = new Mock<HttpContext>();

        public MockHttpContext()
        {
            disposedValue = false;

            var request = new Mock<HttpRequest>();
            request.SetupGet(x => x.Headers).Returns(requestHeaders);
            request.SetupGet(x => x.Body).Returns(requestBody);

            var response = new Mock<HttpResponse>();
            response.SetupGet(x => x.Headers).Returns(responseHeaders);
            response.SetupGet(x => x.Body).Returns(responseBody);

            mockContext.SetupGet(x => x.Request).Returns(request.Object);
            mockContext.SetupGet(x => x.Response).Returns(response.Object);
        }

        public void SetHeader(string key, string value)
        {
            requestHeaders.Add(key, value);
        }

        public string GetHeader(string key)
        {
            return responseHeaders[key];
        }

        public void SetBody(string content)
        {
            var bytes = Convert.FromBase64String(content);
            requestBody.Write(bytes, 0, bytes.Length);
            requestBody.Seek(0, SeekOrigin.Begin);
        }

        public string GetBody()
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var bytes = responseBody.ToArray();
            return Convert.ToBase64String(bytes);
        }

        public HttpContext Object => mockContext.Object;

        #region IDisposable Support
        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    requestBody.Dispose();
                    responseBody.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
