using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class AzureIotHubFacility : IDeviceControlFacility
    {
        private readonly IDeviceStatusProvider deviceStatusProvider;
        public AzureIotHubFacility(IDeviceStatusProvider deviceStatusProvider)
        {
            this.deviceStatusProvider = deviceStatusProvider;
        }

        public event EventHandler<Command> CommandReceived;

        public async Task UpdateAlarm(AlarmStatus status)
        {
            
        }

        public async Task UpdateLight(Light light, LightStatus status)
        {
        }

        public async Task UpdateDoorbell(DoorbellStatus status)
        {
        }

        public void Dispose()
        {
        }
    }
}
