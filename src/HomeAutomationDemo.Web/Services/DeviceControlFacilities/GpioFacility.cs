using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using System.Timers;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class GpioFacility : IDeviceControlFacility
    {
        public event EventHandler<Command> CommandReceived;

        private const int pushButtonPin = 7;
        private const int buzzerPin = 0;
        private const int ledPin = 1;

        private const int blinkInterval = 500;

        private readonly Timer alarmTimer;

        private bool blinkStatus = false;

        public GpioFacility()
        {
            alarmTimer = new Timer()
            {
                Interval = blinkInterval,
                AutoReset = true,
                Enabled = false
            };
            alarmTimer.Elapsed += BlinkAlarm;

        }

        private void BlinkAlarm(object sender, EventArgs e)
        {
            blinkStatus = !blinkStatus;
            Console.WriteLine($"GPIO{ledPin} is now {(blinkStatus ? "HIGH": "LOW")}");
            Console.WriteLine($"GPIO{buzzerPin} is now {(blinkStatus ? "HIGH" : "LOW")}");
        }

        public Task UpdateAlarm(AlarmStatus status)
        {
            switch (status)
            {
                case AlarmStatus.On:
                    blinkStatus = true;
                    alarmTimer.Enabled = false;
                    Console.WriteLine($"GPIO{ledPin} is now HIGH");
                    break;
                case AlarmStatus.Off:
                    blinkStatus = false;
                    alarmTimer.Enabled = false;
                    Console.WriteLine($"GPIO{ledPin} is now LOW");
                    Console.WriteLine($"GPIO{buzzerPin} is now LOW");
                    break;
                case AlarmStatus.Active:
                    alarmTimer.Enabled = true;
                    break;
            }
            return Task.CompletedTask;
        }

        public Task UpdateLight(Light light, LightStatus status)
        {
            var gpio = (int)light;
            Console.WriteLine($"GPIO{gpio} is now {(status == LightStatus.On ? "HIGH" : "LOW")}");
            return Task.CompletedTask;
        }

        public Task UpdateDoorbell(DoorbellStatus status)
        {
            var gpio = buzzerPin;
            Console.WriteLine($"GPIO{gpio} is now {(status == DoorbellStatus.On ? "HIGH" : "LOW")}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
