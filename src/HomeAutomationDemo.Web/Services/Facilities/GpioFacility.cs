using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using System.Timers;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;
using HomeAutomationDemo.Model.Telemetry;

namespace HomeAutomationDemo.Web.Services.Facilities
{
    public class GpioFacility : BaseFacility
    {
        private readonly IDeviceStatusProvider deviceStatusProvider;
        
        private readonly GpioPin alarmLedPin = Pi.Gpio.Pin00; //led (ouput)
        private readonly GpioPin buzzerPin = Pi.Gpio.Pin01; //piezo buzzer (output)
        private readonly GpioPin doorlockPin = Pi.Gpio.Pin06; //reed switch (input)
        private readonly GpioPin doorbellPin = Pi.Gpio.Pin07; //push button (input)

        private readonly Dictionary<Light, GpioPin> lightPins = new Dictionary<Light, GpioPin>() //leds (output)
        {
            { Light.Green, Pi.Gpio.Pin02 },
            { Light.Red, Pi.Gpio.Pin03 },
            { Light.Blue, Pi.Gpio.Pin04 },
            { Light.Yellow, Pi.Gpio.Pin05 }
        };

        private const int blinkInterval = 500; //ms
        private const int inputInterval = 100; //ms

        private readonly Timer alarmTimer;
        private readonly Timer inputTimer;

        private bool blinkStatus = false;

        public GpioFacility(IDeviceStatusProvider deviceStatusProvider)
        {
            this.deviceStatusProvider = deviceStatusProvider;
            alarmTimer = new Timer()
            {
                Interval = blinkInterval,
                AutoReset = true,
                Enabled = false
            };
            alarmTimer.Elapsed += BlinkAlarm;

            inputTimer = new Timer()
            {
                Interval = inputInterval,
                AutoReset = true,
                Enabled = true
            };
            inputTimer.Elapsed += ReadInputValues;


            ConfigureGPIOs();
        }

        private void ReadInputValues(object sender, ElapsedEventArgs e)
        {
            //Doorbell
            var doorbellOn = deviceStatusProvider.CurrentStatus.Doorbell == DoorbellStatus.On;
            var doorbellPinOn = doorbellPin.ReadValue() == GpioPinValue.High;
            if (doorbellOn != doorbellPinOn)
            {
                SendCommand(new UpdateDoorbell() { DesiredStatus = doorbellPinOn ? DoorbellStatus.On : DoorbellStatus.Off });
            }

            //Doorlock
            var doorlockPinOn = doorlockPin.ReadValue() == GpioPinValue.High;
            switch (deviceStatusProvider.CurrentStatus.Alarm)
            {
                case AlarmStatus.On:
                    if (!doorlockPinOn)
                    {
                        SendCommand(new UpdateAlarm() { DesiredStatus = AlarmStatus.Active });
                    }
                    break;
                case AlarmStatus.Active:
                    if (doorlockPinOn)
                    {
                        SendCommand(new UpdateAlarm() { DesiredStatus = AlarmStatus.Off });
                    }
                    break;
            }
        }

        private void ConfigureGPIOs()
        {

            foreach (var lightPin in lightPins)
            {
                lightPin.Value.PinMode = GpioPinDriveMode.Output;
                lightPin.Value.Write(GpioPinValue.Low);
            }

            //Read about pull-up and pull-down
            //http://raspi.tv/2013/rpi-gpio-basics-6-using-inputs-and-outputs-together-with-rpi-gpio-pull-ups-and-pull-downs
            doorbellPin.PinMode = GpioPinDriveMode.Input;
            doorbellPin.InputPullMode = GpioPinResistorPullMode.PullDown;

            doorlockPin.PinMode = GpioPinDriveMode.Input;
            doorbellPin.InputPullMode = GpioPinResistorPullMode.PullDown;

            alarmLedPin.PinMode = GpioPinDriveMode.Output;
            alarmLedPin.Write(GpioPinValue.Low);

            //https://raspberrypi.stackexchange.com/questions/53854/driving-pwm-output-frequency
            //https://projects.drogon.net/raspberry-pi/wiringpi/software-pwm-library/
            buzzerPin.PinMode = GpioPinDriveMode.Output;
            buzzerPin.StartSoftPwm(0, 2);

            //If you know how to properly configure hardware PWM, please send a pull request!
            //The buzzer is already set to use GPIO1 (aka pin 12) which supports hardware PWM
            //buzzerPin.PwmClockDivisor = 16; // 1.2 Mhz
            //buzzerPin.PwmRange = 1200000 / 4000; // 4Khz
            //buzzerPin.PwmMode = PwmMode.MarkSign;
            //And now, how to start hardware PWM???
            
        }

        private void BlinkAlarm(object sender, EventArgs e)
        {
            blinkStatus = !blinkStatus;
            buzzerPin.SoftPwmValue = blinkStatus ? 1 : 0;
            alarmLedPin.Write(blinkStatus ? GpioPinValue.High : GpioPinValue.Low);
        }

        protected override Task HandleAlarmCommand(UpdateAlarm alarmCommand)
        {
            switch (alarmCommand.DesiredStatus)
            {
                case AlarmStatus.On:
                    blinkStatus = true;
                    alarmTimer.Enabled = false;
                    alarmLedPin.Write(GpioPinValue.High);
                    break;
                case AlarmStatus.Off:
                    blinkStatus = false;
                    alarmTimer.Enabled = false;
                    alarmLedPin.Write(GpioPinValue.Low);
                    buzzerPin.SoftPwmValue = 0;
                    break;
                case AlarmStatus.Active:
                    alarmTimer.Enabled = true;
                    break;
            }
            SendTelemetry(new AlarmUpdated { Status = alarmCommand.DesiredStatus });
            return Task.CompletedTask;
        }

        protected override Task HandleLightCommand(UpdateLight lightCommand)
        {
            var gpio = lightPins[lightCommand.Light];
            gpio.Write(lightCommand.DesiredStatus == LightStatus.On ? GpioPinValue.High : GpioPinValue.Low);
            SendTelemetry(new LightUpdated { Light = lightCommand.Light, Status = lightCommand.DesiredStatus });
            return Task.CompletedTask;
        }

        protected override Task HandleDoorbellCommand(UpdateDoorbell doorbellCommand)
        {
            buzzerPin.SoftPwmValue = doorbellCommand.DesiredStatus == DoorbellStatus.On ? 1 : 0;
            SendTelemetry(new DoorbellUpdated { Status = doorbellCommand.DesiredStatus });
            return Task.CompletedTask;
        }
    }
}
