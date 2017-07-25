// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class DeviceGroupControllerTest
    {
        private readonly Mock<IStorage> mockStorage;
        private readonly DeviceGroupController controller;
        private readonly Random rand;

        public DeviceGroupControllerTest()
        {
            mockStorage = new Mock<IStorage>();
            controller = new DeviceGroupController(mockStorage.Object);
            rand = new Random();
        }

        [Fact]
        public async Task GetAllAsyncTest()
        {
            var models = new[]
            {
                new DeviceGroupServiceModel
                {
                    Id = rand.NextString(),
                    DisplayName = rand.NextString(),
                    Conditions = rand.NextString(),
                    ETag = rand.NextString()
                },
                new DeviceGroupServiceModel
                {
                    Id = rand.NextString(),
                    DisplayName = rand.NextString(),
                    Conditions = rand.NextString(),
                    ETag = rand.NextString()
                },
                new DeviceGroupServiceModel
                {
                    Id = rand.NextString(),
                    DisplayName = rand.NextString(),
                    Conditions = rand.NextString(),
                    ETag = rand.NextString()
                }
            };

            mockStorage
                .Setup(x => x.GetAllDeviceGroupsAsync())
                .ReturnsAsync(models);

            var result = await controller.GetAllAsync();

            mockStorage
                .Verify(x => x.GetAllDeviceGroupsAsync(), Times.Once);

            Assert.Equal(result.Items.Count(), models.Length);
            foreach (var item in result.Items)
            {
                var model = models.Single(g => g.Id == item.Id);
                Assert.Equal(model.DisplayName, item.DisplayName);
                Assert.Equal(model.Conditions, item.Conditions);
                Assert.Equal(model.ETag, item.ETag);
            }
        }

        [Fact]
        public async Task GetAsyncTest()
        {
            var groupId = rand.NextString();
            var displayName = rand.NextString();
            var conditions = rand.NextString();
            var etag = rand.NextString();

            mockStorage
                .Setup(x => x.GetDeviceGroupAsync(It.IsAny<string>()))
                .ReturnsAsync(new DeviceGroupServiceModel
                {
                    Id = groupId,
                    DisplayName = displayName,
                    Conditions = conditions,
                    ETag = etag
                });

            var result = await controller.GetAsync(groupId);

            mockStorage
                .Verify(x => x.GetDeviceGroupAsync(
                    It.Is<string>(s => s == groupId)),
                    Times.Once);

            Assert.Equal(result.DisplayName, displayName);
            Assert.Equal(result.Conditions, conditions);
            Assert.Equal(result.ETag, etag);
        }

        [Fact]
        public async Task CreatAsyncTest()
        {
            var groupId = rand.NextString();
            var displayName = rand.NextString();
            var conditions = rand.NextString();
            var etag = rand.NextString();

            mockStorage
                .Setup(x => x.CreateDeviceGroupAsync(It.IsAny<DeviceGroupServiceModel>()))
                .ReturnsAsync(new DeviceGroupServiceModel
                {
                    Id = groupId,
                    DisplayName = displayName,
                    Conditions = conditions,
                    ETag = etag
                });

            var result = await controller.CreateAsync(new DeviceGroupApiModel
            {
                DisplayName = displayName,
                Conditions = conditions
            });

            mockStorage
                .Verify(x => x.CreateDeviceGroupAsync(
                    It.Is<DeviceGroupServiceModel>(m => m.DisplayName == displayName && m.Conditions.ToString() == conditions)),
                    Times.Once);

            Assert.Equal(result.Id, groupId);
            Assert.Equal(result.DisplayName, displayName);
            Assert.Equal(result.Conditions, conditions);
            Assert.Equal(result.ETag, etag);
        }

        [Fact]
        public async Task UpdateAsyncTest()
        {
            var groupId = rand.NextString();
            var displayName = rand.NextString();
            var conditions = rand.NextString();
            var etagOld = rand.NextString();
            var etagNew = rand.NextString();

            mockStorage
                .Setup(x => x.UpdateDeviceGroupAsync(It.IsAny<string>(), It.IsAny<DeviceGroupServiceModel>(), It.IsAny<string>()))
                .ReturnsAsync(new DeviceGroupServiceModel
                {
                    Id = groupId,
                    DisplayName = displayName,
                    Conditions = conditions,
                    ETag = etagNew
                });

            var result = await controller.UpdateAsync(groupId,
                new DeviceGroupApiModel
                {
                    DisplayName = displayName,
                    Conditions = conditions,
                    ETag = etagOld
                });

            mockStorage
                .Verify(x => x.UpdateDeviceGroupAsync(
                    It.Is<string>(s => s == groupId),
                    It.Is<DeviceGroupServiceModel>(m => m.DisplayName == displayName && m.Conditions.ToString() == conditions),
                    It.Is<string>(s => s == etagOld)),
                    Times.Once);

            Assert.Equal(result.Id, groupId);
            Assert.Equal(result.DisplayName, displayName);
            Assert.Equal(result.Conditions, conditions);
            Assert.Equal(result.ETag, etagNew);
        }

        [Fact]
        public async Task DeleteAsyncTest()
        {
            var groupId = rand.NextString();

            mockStorage
                .Setup(x => x.DeleteDeviceGroupAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            await controller.DeleteAsync(groupId);

            mockStorage
                .Verify(x => x.DeleteDeviceGroupAsync(
                    It.Is<string>(s => s == groupId)),
                    Times.Once);
        }
    }
}
