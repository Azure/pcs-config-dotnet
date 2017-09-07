using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface ISimulationServiceClient
    {
        Task<HashSet<string>> GetDevicePropertyNamesAsync();
    }
}
