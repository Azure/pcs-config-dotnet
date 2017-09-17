// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class SolutionControllerTest
    {
        private readonly Mock<IStorage> mockStorage;
        private readonly SolutionSettingsController controller;
        private readonly Random rand;

        public SolutionControllerTest()
        {
            this.mockStorage = new Mock<IStorage>();
            this.controller = new SolutionSettingsController(this.mockStorage.Object);
            this.rand = new Random();
        }

        [Fact]
        public async Task GetThemeAsyncTest()
        {
            var name = this.rand.NextString();
            var description = this.rand.NextString();

            this.mockStorage
                .Setup(x => x.GetThemeAsync())
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await this.controller.GetThemeAsync() as dynamic;

            this.mockStorage
                .Verify(x => x.GetThemeAsync(), Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task SetThemeAsyncTest()
        {
            var name = this.rand.NextString();
            var description = this.rand.NextString();

            this.mockStorage
                .Setup(x => x.SetThemeAsync(It.IsAny<object>()))
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await this.controller.SetThemeAsync(new
            {
                Name = name,
                Description = description
            }) as dynamic;

            this.mockStorage
                .Verify(x => x.SetThemeAsync(
                    It.Is<object>(o => this.CheckTheme(o, name, description))),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        private bool CheckTheme(object obj, string name, string description)
        {
            var dynamiceObj = obj as dynamic;
            return dynamiceObj.Name.ToString() == name && dynamiceObj.Description.ToString() == description;
        }

        [Fact]
        public async Task GetLogoAsyncTest()
        {
            var image = this.rand.NextString();
            var type = this.rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                this.mockStorage
                    .Setup(x => x.GetLogoAsync())
                    .ReturnsAsync(new Logo
                    {
                        Image = image,
                        Type = type
                    });

                await this.controller.GetLogoAsync();

                this.mockStorage
                    .Verify(x => x.GetLogoAsync(), Times.Once);

                Assert.Equal(mockContext.GetBody(), image);
                Assert.Equal(mockContext.GetHeader("content-type"), type);
            }
        }

        [Fact]
        public async Task SetLogoAsyncTest()
        {
            var image = this.rand.NextString();
            var type = this.rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                this.controller.ControllerContext.HttpContext = mockContext.Object;

                this.mockStorage
                    .Setup(x => x.SetLogoAsync(It.IsAny<Logo>()))
                    .ReturnsAsync(new Logo
                    {
                        Image = image,
                        Type = type
                    });

                mockContext.SetHeader("content-type", type);
                mockContext.SetBody(image);
                await this.controller.SetLogoAsync();

                this.mockStorage
                    .Verify(x => x.SetLogoAsync(
                        It.Is<Logo>(m => m.Image == image && m.Type == type)),
                        Times.Once);

                Assert.Equal(mockContext.GetBody(), image);
                Assert.Equal(mockContext.GetHeader("content-type"), type);
            }
        }
    }
}
