// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class SettingsServiceModel
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public static readonly SettingsServiceModel Default = new SettingsServiceModel
        {
            Name = "My Solution",
            Description = "My Solution Description"
        };
    }
}
