using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Status
{
    public class DeviceStatus
    {
        public DeviceStatus(IEnumerable<KeyValuePair<Light, LightStatus>> lights, DoorbellStatus doorbellStatus, AlarmStatus alarmStatus)
        {
            Lights = lights;
            Doorbell = doorbellStatus;
            Alarm = alarmStatus;
        }

        private DeviceStatus()
        {

        }

        public IEnumerable<KeyValuePair<Light, LightStatus>> Lights { get; }
        public DoorbellStatus Doorbell { get; }
        public AlarmStatus Alarm { get; }
    }
}
