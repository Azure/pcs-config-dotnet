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
            mockStorage = new Mock<IStorage>();
            controller = new SolutionSettingsController(mockStorage.Object);
            rand = new Random();
        }

        [Fact]
        public async Task GetThemeAsyncTest()
        {
            var name = rand.NextString();
            var description = rand.NextString();

            mockStorage
                .Setup(x => x.GetThemeAsync())
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await controller.GetThemeAsync() as dynamic;

            mockStorage
                .Verify(x => x.GetThemeAsync(), Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task SetThemeAsyncTest()
        {
            var name = rand.NextString();
            var description = rand.NextString();

            mockStorage
                .Setup(x => x.SetThemeAsync(It.IsAny<object>()))
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await controller.SetThemeAsync(new
            {
                Name = name,
                Description = description
            }) as dynamic;

            mockStorage
                .Verify(x => x.SetThemeAsync(
                    It.Is<object>(o => CheckTheme(o, name, description))),
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
            var image = rand.NextString();
            var type = rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                controller.ControllerContext.HttpContext = mockContext.Object;

                mockStorage
                    .Setup(x => x.GetLogoAsync())
                    .ReturnsAsync(new LogoServiceModel
                    {
                        Image = image,
                        Type = type
                    });

                await controller.GetLogoAsync();

                mockStorage
                    .Verify(x => x.GetLogoAsync(), Times.Once);

                Assert.Equal(mockContext.GetBody(), image);
                Assert.Equal(mockContext.GetHeader("content-type"), type);
            }
        }

        [Fact]
        public async Task SetLogoAsyncTest()
        {
            var image = rand.NextString();
            var type = rand.NextString();

            using (var mockContext = new MockHttpContext())
            {
                controller.ControllerContext.HttpContext = mockContext.Object;

                mockStorage
                    .Setup(x => x.SetLogoAsync(It.IsAny<LogoServiceModel>()))
                    .ReturnsAsync(new LogoServiceModel
                    {
                        Image = image,
                        Type = type
                    });

                mockContext.SetHeader("content-type", type);
                mockContext.SetBody(image);
                await controller.SetLogoAsync();

                mockStorage
                    .Verify(x => x.SetLogoAsync(
                        It.Is<LogoServiceModel>(m => m.Image == image && m.Type == type)),
                        Times.Once);

                Assert.Equal(mockContext.GetBody(), image);
                Assert.Equal(mockContext.GetHeader("content-type"), type);
            }
        }
    }
}
