using HomeAutomationDemo.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using HomeAutomationDemo.Model.Status;
using HomeAutomationDemo.Model.Enums;
using HomeAutomationDemo.Model.Commands;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Services.DeviceManager {
    public class LocalDeviceManager : IDeviceManager
    {
        private readonly IServiceProvider serviceProvider;
        private readonly List<IDeviceControlFacility> deviceFacilities;

        //Statuses
        private readonly Dictionary<Light, LightStatus> lights;
        private DoorbellStatus doorbellStatus = DoorbellStatus.Off;
        private AlarmStatus alarmStatus = AlarmStatus.Off;

        public LocalDeviceManager(IServiceProvider serviceProvider)
        {
            deviceFacilities = new List<IDeviceControlFacility>();
            lights = new Dictionary<Light, LightStatus>() { { Light.Blue, LightStatus.Off }, { Light.Green, LightStatus.Off }, { Light.Red, LightStatus.Off }, { Light.Yellow, LightStatus.Off } };
            this.serviceProvider = serviceProvider;
            UpdateCurrentStatus();
        }

        private void UpdateCurrentStatus()
        {
            CurrentStatus = new DeviceStatus(lights, doorbellStatus, alarmStatus);
        }

        public DeviceStatus CurrentStatus
        {
            get; private set;
        }

        public void Init()
        {
            deviceFacilities.AddRange(serviceProvider.GetServices<IDeviceControlFacility>());
            foreach (var facility in deviceFacilities)
            {
                facility.CommandReceived += HandleDeviceCommand;
            }
        }

        #region Command handlers
        private async void HandleDeviceCommand(object sender, Command command)
        {
            switch (command)
            {
                case UpdateLight updateLightCommand:
                    await HandleUpdateLightCommand(updateLightCommand);
                    break;
                case UpdateDoorbell updateDoorbellCommand:
                    await HandleDoorbellCommand(updateDoorbellCommand);
                    break;
                case UpdateAlarm updateAlarmCommand:
                    await HandleUpdateAlarmCommand(updateAlarmCommand);
                    break;
            }
        }

        private async Task HandleUpdateLightCommand(UpdateLight updateLightCommand)
        {

            if (!lights.ContainsKey(updateLightCommand.Light))
            {
                return;
            }

            if (lights[updateLightCommand.Light] != updateLightCommand.DesiredStatus)
            {
                lights[updateLightCommand.Light] = updateLightCommand.DesiredStatus;
                UpdateCurrentStatus();
                foreach (var facility in deviceFacilities)
                {
                    await facility.UpdateLight(updateLightCommand.Light, updateLightCommand.DesiredStatus);
                }
            }
        }

        private async Task HandleDoorbellCommand(UpdateDoorbell updateDoorbellCommand)
        {
            if (doorbellStatus != updateDoorbellCommand.DesiredStatus)
            {
                doorbellStatus = updateDoorbellCommand.DesiredStatus;
                UpdateCurrentStatus();
                foreach (var facility in deviceFacilities)
                {
                    await facility.UpdateDoorbell(updateDoorbellCommand.DesiredStatus);
                }
            }
        }

        private async Task HandleUpdateAlarmCommand(UpdateAlarm updateAlarmCommand)
        {
            if (alarmStatus != updateAlarmCommand.DesiredStatus)
            {
                alarmStatus = updateAlarmCommand.DesiredStatus;
                UpdateCurrentStatus();
                foreach (var facility in deviceFacilities)
                {
                    await facility.UpdateAlarm(updateAlarmCommand.DesiredStatus);
                }
            }
        }
        #endregion

        public void Dispose()
        {
            foreach (var facility in deviceFacilities)
            {
                facility.CommandReceived -= HandleDeviceCommand;
                facility.Dispose();
            }
        }


    }
}