// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class StorageTest
    {
        private readonly Mock<IStorageAdapterClient> mockClient;
        private readonly Storage storage;
        private readonly Random rand;

        public StorageTest()
        {
            mockClient = new Mock<IStorageAdapterClient>();
            storage = new Storage(mockClient.Object);
            rand = new Random();
        }

        [Fact]
        public async Task GetThemeAsyncTest()
        {
            var name = rand.NextString();
            var description = rand.NextString();

            mockClient
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Data = JsonConvert.SerializeObject(new
                    {
                        Name = name,
                        Description = description
                    })
                });

            var result = await storage.GetThemeAsync() as dynamic;

            mockClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Storage.SolutionCollectionId),
                    It.Is<string>(s => s == Storage.ThemeKey)),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task GetThemeAsyncDefaultTest()
        {
            mockClient
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ResourceNotFoundException());

            var result = await storage.GetThemeAsync() as dynamic;

            mockClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Storage.SolutionCollectionId),
                    It.Is<string>(s => s == Storage.ThemeKey)),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), ThemeServiceModel.Default.Name);
            Assert.Equal(result.Description.ToString(), ThemeServiceModel.Default.Description);
        }

        [Fact]
        public async Task SetThemeAsyncTest()
        {
            var name = rand.NextString();
            var description = rand.NextString();

            var theme = new
            {
                Name = name,
                Description = description
            };

            mockClient
                .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Data = JsonConvert.SerializeObject(theme)
                });

            var result = await storage.SetThemeAsync(theme) as dynamic;

            mockClient
                .Verify(x => x.UpdateAsync(
                    It.Is<string>(s => s == Storage.SolutionCollectionId),
                    It.Is<string>(s => s == Storage.ThemeKey),
                    It.Is<string>(s => s == JsonConvert.SerializeObject(theme)),
                    It.Is<string>(s => s == "*")),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task GetUserSettingAsyncTest()
        {
            var id = this.rand.NextString();
            var name = rand.NextString();
            var description = rand.NextString();

            mockClient
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Data = JsonConvert.SerializeObject(new
                    {
                        Name = name,
                        Description = description
                    })
                });

            var result = await storage.GetUserSetting(id) as dynamic;

            mockClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Storage.UserCollectionId),
                    It.Is<string>(s => s == id)),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task SetUserSettingAsyncTest()
        {
            var id = this.rand.NextString();
            var name = rand.NextString();
            var description = rand.NextString();

            var setting = new
            {
                Name = name,
                Description = description
            };

            mockClient
                .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Data = JsonConvert.SerializeObject(setting)
                });

            var result = await storage.SetUserSetting(id, setting) as dynamic;

            mockClient
                .Verify(x => x.UpdateAsync(
                        It.Is<string>(s => s == Storage.UserCollectionId),
                        It.Is<string>(s => s == id),
                        It.Is<string>(s => s == JsonConvert.SerializeObject(setting)),
                        It.Is<string>(s => s == "*")),
                    Times.Once);

            Assert.Equal(result.Name.ToString(), name);
            Assert.Equal(result.Description.ToString(), description);
        }

        [Fact]
        public async Task GetLogoAsyncTest()
        {
            var image = rand.NextString();
            var type = rand.NextString();

            mockClient
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Data = JsonConvert.SerializeObject(new LogoServiceModel
                    {
                        Image = image,
                        Type = type
                    })
                });

            var result = await storage.GetLogoAsync() as dynamic;

            mockClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Storage.SolutionCollectionId),
                    It.Is<string>(s => s == Storage.LogoKey)),
                    Times.Once);

            Assert.Equal(result.Image.ToString(), image);
            Assert.Equal(result.Type.ToString(), type);
        }

        [Fact]
        public async Task GetLogoAsyncDefaultTest()
        {
            mockClient
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ResourceNotFoundException());

            var result = await storage.GetLogoAsync() as dynamic;

            mockClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Storage.SolutionCollectionId),
                    It.Is<string>(s => s == Storage.LogoKey)),
                    Times.Once);

            Assert.Equal(result.Image.ToString(), LogoServiceModel.Default.Image);
            Assert.Equal(result.Type.ToString(), LogoServiceModel.Default.Type);
        }

        [Fact]
        public async Task SetLogoAsyncTest()
        {
            var image = rand.NextString();
            var type = rand.NextString();

            var logo = new LogoServiceModel
            {
                Image = image,
                Type = type
            };

            mockClient
                .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Data = JsonConvert.SerializeObject(logo)
                });

            var result = await storage.SetLogoAsync(logo) as dynamic;

            mockClient
                .Verify(x => x.UpdateAsync(
                    It.Is<string>(s => s == Storage.SolutionCollectionId),
                    It.Is<string>(s => s == Storage.LogoKey),
                    It.Is<string>(s => s == JsonConvert.SerializeObject(logo)),
                    It.Is<string>(s => s == "*")),
                    Times.Once);

            Assert.Equal(result.Image.ToString(), image);
            Assert.Equal(result.Type.ToString(), type);
        }

        [Fact]
        public async Task GetAllDeviceGroupsAsyncTest()
        {
            var groups = new[]
            {
                new DeviceGroupServiceModel
                {
                    DisplayName = rand.NextString(),
                    Conditions = new List<DeviceGroupConditionModel>()
                    {
                        new DeviceGroupConditionModel()
                        {
                            Key = rand.NextString(),
                            Operator = OperatorType.EQ,
                            Value = rand.NextString()
                        }
                    }
                },
                new DeviceGroupServiceModel
                {
                    DisplayName = rand.NextString(),
                    Conditions = new List<DeviceGroupConditionModel>()
                    {
                        new DeviceGroupConditionModel()
                        {
                            Key = rand.NextString(),
                            Operator = OperatorType.EQ,
                            Value = rand.NextString()
                        }
                    }
                },
                new DeviceGroupServiceModel
                {
                    DisplayName = rand.NextString(),
                    Conditions = new List<DeviceGroupConditionModel>()
                    {
                        new DeviceGroupConditionModel()
                        {
                            Key = rand.NextString(),
                            Operator = OperatorType.EQ,
                            Value = rand.NextString()
                        }
                    }
                }
            };

            var items = groups.Select(g => new ValueApiModel
            {
                Key = rand.NextString(),
                Data = JsonConvert.SerializeObject(g),
                ETag = rand.NextString()
            }).ToList();

            mockClient
                .Setup(x => x.GetAllAsync(It.IsAny<string>()))
                .ReturnsAsync(new ValueListApiModel { Items = items });

            var result = (await storage.GetAllDeviceGroupsAsync()).ToList();

            mockClient
                .Verify(x => x.GetAllAsync(
                    It.Is<string>(s => s == Storage.DeviceGroupCollectionId)),
                    Times.Once);

            Assert.Equal(result.Count, groups.Length);
            foreach (var g in result)
            {
                var item = items.Single(i => i.Key == g.Id);
                var group = JsonConvert.DeserializeObject<DeviceGroupServiceModel>(item.Data);
                Assert.Equal(g.DisplayName, group.DisplayName);
                Assert.Equal(g.Conditions.First().Key, group.Conditions.First().Key);
                Assert.Equal(g.Conditions.First().Operator, group.Conditions.First().Operator);
                Assert.Equal(g.Conditions.First().Value, group.Conditions.First().Value);
            }
        }

        [Fact]
        public async Task GetDeviceGroupsAsyncTest()
        {
            var groupId = rand.NextString();
            var displayName = rand.NextString();
            var conditions = new List<DeviceGroupConditionModel>()
            {
                new DeviceGroupConditionModel()
                {
                    Key = rand.NextString(),
                    Operator = OperatorType.EQ,
                    Value = rand.NextString()
                }
            };
            var etag = rand.NextString();

            mockClient
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Key = groupId,
                    Data = JsonConvert.SerializeObject(new DeviceGroupServiceModel
                    {
                        DisplayName = displayName,
                        Conditions = conditions
                    }),
                    ETag = etag
                });

            var result = await storage.GetDeviceGroupAsync(groupId);

            mockClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == Storage.DeviceGroupCollectionId),
                    It.Is<string>(s => s == groupId)),
                    Times.Once);

            Assert.Equal(result.DisplayName, displayName);
            Assert.Equal(result.Conditions.First().Key, conditions.First().Key);
            Assert.Equal(result.Conditions.First().Operator, conditions.First().Operator);
            Assert.Equal(result.Conditions.First().Value, conditions.First().Value);
        }

        [Fact]
        public async Task CreateDeviceGroupAsyncTest()
        {
            var groupId = rand.NextString();
            var displayName = rand.NextString();
            var conditions = new List<DeviceGroupConditionModel>()
            {
                new DeviceGroupConditionModel()
                {
                    Key = rand.NextString(),
                    Operator = OperatorType.EQ,
                    Value = rand.NextString()
                }
            };
            var etag = rand.NextString();

            var group = new DeviceGroupServiceModel
            {
                DisplayName = displayName,
                Conditions = conditions
            };

            mockClient
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Key = groupId,
                    Data = JsonConvert.SerializeObject(group),
                    ETag = etag
                });

            var result = await storage.CreateDeviceGroupAsync(group);

            mockClient
                .Verify(x => x.CreateAsync(
                    It.Is<string>(s => s == Storage.DeviceGroupCollectionId),
                    It.Is<string>(s => s == JsonConvert.SerializeObject(group, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))),
                    Times.Once);

            Assert.Equal(result.Id, groupId);
            Assert.Equal(result.DisplayName, displayName);
            Assert.Equal(result.Conditions.First().Key, conditions.First().Key);
            Assert.Equal(result.Conditions.First().Operator, conditions.First().Operator);
            Assert.Equal(result.Conditions.First().Value, conditions.First().Value);
            Assert.Equal(result.ETag, etag);
        }

        [Fact]
        public async Task UpdateDeviceGroupAsyncTest()
        {
            var groupId = rand.NextString();
            var displayName = rand.NextString();
            var conditions = new List<DeviceGroupConditionModel>()
            {
                new DeviceGroupConditionModel()
                {
                    Key = rand.NextString(),
                    Operator = OperatorType.EQ,
                    Value = rand.NextString()
                }
            };
            var etagOld = rand.NextString();
            var etagNew = rand.NextString();

            var group = new DeviceGroupServiceModel
            {
                DisplayName = displayName,
                Conditions = conditions
            };

            mockClient
                .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueApiModel
                {
                    Key = groupId,
                    Data = JsonConvert.SerializeObject(group),
                    ETag = etagNew
                });

            var result = await storage.UpdateDeviceGroupAsync(groupId, group, etagOld);

            mockClient
                .Verify(x => x.UpdateAsync(
                    It.Is<string>(s => s == Storage.DeviceGroupCollectionId),
                    It.Is<string>(s => s == groupId),
                    It.Is<string>(s => s == JsonConvert.SerializeObject(group, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })),
                    It.Is<string>(s => s == etagOld)),
                    Times.Once);

            Assert.Equal(result.Id, groupId);
            Assert.Equal(result.DisplayName, displayName);
            Assert.Equal(result.Conditions.First().Key, conditions.First().Key);
            Assert.Equal(result.Conditions.First().Operator, conditions.First().Operator);
            Assert.Equal(result.Conditions.First().Value, conditions.First().Value);
            Assert.Equal(result.ETag, etagNew);
        }

        [Fact]
        public async Task DeleteDeviceGroupAsyncTest()
        {
            var groupId = rand.NextString();

            mockClient
                .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            await storage.DeleteDeviceGroupAsync(groupId);

            mockClient
                .Verify(x => x.DeleteAsync(
                    It.Is<string>(s => s == Storage.DeviceGroupCollectionId),
                    It.Is<string>(s => s == groupId)),
                    Times.Once);
        }
    }
}
