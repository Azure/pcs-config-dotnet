// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class UserSettingsControllerTest
    {
        private readonly Mock<IStorage> mockStorage;
        private readonly UserSettingsController controller;
        private readonly Random rand;

        public UserSettingsControllerTest()
        {
            mockStorage = new Mock<IStorage>();
            controller = new UserSettingsController(mockStorage.Object);
            rand = new Random();
        }

        [Fact]
        public async Task GetUserSettingAsyncTest()
        {
            var id = this.rand.NextString();
            var name = rand.NextString();
            var description = rand.NextString();

            mockStorage
                .Setup(x => x.GetUserSetting(It.IsAny<string>()))
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await controller.GetUserSettingAsync(id) as dynamic;

            mockStorage
                .Verify(x => x.GetUserSetting(
                    It.Is<string>(s => s == id)), Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task SetUserSettingAsyncTest()
        {
            var id = this.rand.NextString();
            var name = rand.NextString();
            var description = rand.NextString();

            mockStorage
                .Setup(x => x.SetUserSetting(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new
                {
                    Name = name,
                    Description = description
                });

            var result = await controller.SetUserSettingAsync(id, new
            {
                Name = name,
                Description = description
            }) as dynamic;

            mockStorage
                .Verify(x => x.SetUserSetting(
                        It.Is<string>(s => s == id),
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
    }
}
