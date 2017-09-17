// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class Logo
    {
        public string Image { get; set; }
        public string Type { get; set; }
        
        public static readonly Logo Default;

        static Logo()
        {
            var folder = Path.GetDirectoryName(typeof(Logo).GetTypeInfo().Assembly.Location);
            var path = $@"{folder}/Content/DefaultLogo.svg";
            var bytes = File.ReadAllBytes(path);
            Default = new Logo
            {
                Image = Convert.ToBase64String(bytes),
                Type = "image/svg+xml"
            };
        }
    }
}
