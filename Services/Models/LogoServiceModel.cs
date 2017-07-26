// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Reflection;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class LogoServiceModel
    {
        public string Image { get; set; }
        public string Type { get; set; }

        public static readonly LogoServiceModel Default;

        static LogoServiceModel()
        {
            var folder = Path.GetDirectoryName(typeof(LogoServiceModel).GetTypeInfo().Assembly.Location);
            var path = $@"{folder}/Content/DefaultLogo.svg";
            var bytes = File.ReadAllBytes(path);
            Default = new LogoServiceModel
            {
                Image = System.Convert.ToBase64String(bytes),
                Type = "image/svg+xml"
            };
        }
    }
}
