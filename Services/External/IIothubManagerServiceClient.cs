using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IIothubManagerServiceClient
    {
        Task<DeviceTwinName> GetDeviceTwinNamesAsync();
    }
}
