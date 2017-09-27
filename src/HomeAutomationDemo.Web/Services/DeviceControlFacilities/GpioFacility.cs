using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using System.Timers;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class GpioFacility : IDeviceControlFacility
    {
        public event EventHandler<Command> CommandReceived;

        private readonly GpioPin pushButtonPin = Pi.Gpio.Pin00;
        private readonly GpioPin buzzerPin = Pi.Gpio.Pin01;
        private readonly GpioPin sirenPin = Pi.Gpio.Pin07;
        private readonly Dictionary<Light, GpioPin> lightPins = new Dictionary<Light, GpioPin>()
        {
            { Light.Red, Pi.Gpio.Pin02 },
            { Light.Yellow, Pi.Gpio.Pin03 },
            { Light.Blue, Pi.Gpio.Pin04 },
            { Light.Green, Pi.Gpio.Pin05 }
        };


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
            ConfigureGPIOs();
        }

        private void ConfigureGPIOs()
        {
            foreach (var lightPin in lightPins)
            {
                lightPin.Value.PinMode = GpioPinDriveMode.Output;
                lightPin.Value.Write(GpioPinValue.Low);
            }
            pushButtonPin.PinMode = GpioPinDriveMode.Input;
            sirenPin.PinMode = GpioPinDriveMode.Output;
            sirenPin.Write(GpioPinValue.Low);

            //https://raspberrypi.stackexchange.com/questions/53854/driving-pwm-output-frequency
            //https://projects.drogon.net/raspberry-pi/wiringpi/software-pwm-library/
            buzzerPin.PinMode = GpioPinDriveMode.Output;
            buzzerPin.StartSoftPwm(0, 2);
            //buzzerPin.PwmClockDivisor = 16; // 1.2 Mhz
            //buzzerPin.PwmRange = 1200000 / 4000; // 4Khz
            //buzzerPin.PwmMode = PwmMode.MarkSign;
            
        }

        private void BlinkAlarm(object sender, EventArgs e)
        {
            blinkStatus = !blinkStatus;
            if (blinkStatus)
            {
                buzzerPin.SoftPwmValue = 1;
                sirenPin.Write(GpioPinValue.High);
            } else
            {
                buzzerPin.SoftPwmValue = 0;
                sirenPin.Write(GpioPinValue.Low);
            }
            //Console.WriteLine($"GPIO{sirenPin} is now {(blinkStatus ? "HIGH": "LOW")}");
            //Console.WriteLine($"GPIO{buzzerPin} is now {(blinkStatus ? "HIGH" : "LOW")}");
        }

        public Task UpdateAlarm(AlarmStatus status)
        {
            switch (status)
            {
                case AlarmStatus.On:
                    blinkStatus = true;
                    alarmTimer.Enabled = false;
                    sirenPin.Write(GpioPinValue.High);
                    //Console.WriteLine($"GPIO{sirenPin} is now HIGH");
                    break;
                case AlarmStatus.Off:
                    blinkStatus = false;
                    alarmTimer.Enabled = false;
                    sirenPin.Write(GpioPinValue.Low);
                    buzzerPin.SoftPwmValue = 0;
                    //Console.WriteLine($"GPIO{sirenPin} is now LOW");
                    //Console.WriteLine($"GPIO{buzzerPin} is now LOW");
                    break;
                case AlarmStatus.Active:
                    alarmTimer.Enabled = true;
                    break;
            }
            return Task.CompletedTask;
        }

        public Task UpdateLight(Light light, LightStatus status)
        {
            var gpio = lightPins[light];
            gpio.Write(status == LightStatus.On ? GpioPinValue.High : GpioPinValue.Low);
            //Console.WriteLine($"GPIO{gpio} is now {(status == LightStatus.On ? "HIGH" : "LOW")}");
            return Task.CompletedTask;
        }

        public Task UpdateDoorbell(DoorbellStatus status)
        {
            if (status == DoorbellStatus.On)
            {
                buzzerPin.SoftPwmValue = 1;
            } else
            {
                buzzerPin.SoftPwmValue = 0;
            }
            //Console.WriteLine($"GPIO{gpio} is now {(status == DoorbellStatus.On ? "HIGH" : "LOW")}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
