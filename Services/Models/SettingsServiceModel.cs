// Copyright (c) Microsoft. All rights reserved.


namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class SettingsServiceModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public static SettingsServiceModel Default = new SettingsServiceModel
        {
            Name = "My Solution",
            Description = "My Solution Description"
        };
    }
}
