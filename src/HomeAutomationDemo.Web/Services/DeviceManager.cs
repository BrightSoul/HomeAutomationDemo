using HomeAutomationDemo.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using HomeAutomationDemo.Model.Status;
using HomeAutomationDemo.Model.Enums;
using HomeAutomationDemo.Model.Commands;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Telemetry;

namespace HomeAutomationDemo.Web.Services.DeviceManager {
    public class DeviceManager : IDeviceManager
    {
        private readonly IServiceProvider serviceProvider;
        private readonly List<BaseFacility> facilities;

        //Statuses
        private readonly Dictionary<Light, LightStatus> lights;
        private DoorbellStatus doorbellStatus = DoorbellStatus.Off;
        private AlarmStatus alarmStatus = AlarmStatus.Off;

        public DeviceManager(IServiceProvider serviceProvider)
        {
            facilities = new List<BaseFacility>();
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
            facilities.AddRange(serviceProvider.GetServices<BaseFacility>());
            foreach (var facility in facilities)
            {
                facility.CommandReceived += HandleCommand;
                facility.TelemetryReceived += HandleTelemetry;
            }
        }

        #region Command and telemetry handlers
        private async void HandleCommand(object sender, Command command)
        {
            foreach (var facility in facilities)
            {
                await facility.HandleCommand(command);
            }
        }

        private async void HandleTelemetry(object sender, Telemetry telemetry)
        {
            foreach (var facility in facilities)
            {
                await facility.HandleTelemetry(telemetry);
            }
            UpdateCurrentStatus(telemetry);

        }

        private void UpdateCurrentStatus(Telemetry telemetry)
        {
            switch (telemetry)
            {
                case LightUpdated lightTelemetry:
                    HandleLightTelemetry(lightTelemetry);
                    break;
                case DoorbellUpdated doorbellTelemtry:
                    HandleDoorbellTelemetry(doorbellTelemtry);
                    break;
                case AlarmUpdated alarmTelemetry:
                    HandleAlarmTelemetry(alarmTelemetry);
                    break;
            }
        }

        private void HandleLightTelemetry(LightUpdated lightTelemetry)
        {
            if (!lights.ContainsKey(lightTelemetry.Light))
            {
                return;
            }

            if (lights[lightTelemetry.Light] != lightTelemetry.Status)
            {
                lights[lightTelemetry.Light] = lightTelemetry.Status;
                UpdateCurrentStatus();
            }
        }

        private void HandleDoorbellTelemetry(DoorbellUpdated doorbellTelemetry)
        {
            if (doorbellStatus != doorbellTelemetry.Status)
            {
                doorbellStatus = doorbellTelemetry.Status;
                UpdateCurrentStatus();
            }
        }

        private void HandleAlarmTelemetry(AlarmUpdated alarmTelemetry)
        {
            if (alarmStatus != alarmTelemetry.Status)
            {
                alarmStatus = alarmTelemetry.Status;
                UpdateCurrentStatus();
            }
        }
        #endregion

        public void Dispose()
        {
            foreach (var facility in facilities)
            {
                facility.CommandReceived -= HandleCommand;
                facility.TelemetryReceived -= HandleTelemetry;
                facility.Dispose();
            }
        }


    }
}