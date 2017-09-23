using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class WebsocketFacility : IDeviceControlFacility
    {
        public event EventHandler<Command> CommandReceived;

        private readonly IDeviceManager remoteControlService;
        public WebsocketFacility(IDeviceManager remoteControlService)
        {
            this.remoteControlService = remoteControlService;
        }

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
