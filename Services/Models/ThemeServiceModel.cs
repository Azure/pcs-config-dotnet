// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class ThemeServiceModel
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public static readonly ThemeServiceModel Default = new ThemeServiceModel
        {
            Name = "My Solution",
            Description = "My Solution Description"
        };
    }
}
