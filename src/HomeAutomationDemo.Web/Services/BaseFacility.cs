using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Services
{
    public abstract class BaseFacility : IDisposable
    {
        public event EventHandler<Command> CommandReceived;
        public event EventHandler<Telemetry> TelemetryReceived;

        #region Commands
        public virtual async Task HandleCommand(Command command)
        {
            switch (command)
            {
                case UpdateLight lightCommand:
                    await HandleLightCommand(lightCommand);
                    break;
                case UpdateDoorbell doorbellCommand:
                    await HandleDoorbellCommand(doorbellCommand);
                    break;
                case UpdateAlarm alarmCommand:
                    await HandleAlarmCommand(alarmCommand);
                    break;
            }
        }

        protected virtual Task HandleLightCommand(UpdateLight lightCommand)
        {
            return Task.CompletedTask;
        }

        protected virtual Task HandleDoorbellCommand(UpdateDoorbell doorbellCommand)
        {
            return Task.CompletedTask;
        }

        protected virtual Task HandleAlarmCommand(UpdateAlarm alarmCommand)
        {
            return Task.CompletedTask;
        }
        protected void SendCommand(Command command)
        {
            CommandReceived?.Invoke(this, command);
        }
        #endregion

        #region Telemetry
        public async virtual Task HandleTelemetry(Telemetry telemetry)
        {
            switch (telemetry)
            {
                case LightUpdated lightTelemetry:
                    await HandleLightTelemetry(lightTelemetry);
                    break;
                case DoorbellUpdated doorbellTelemtry:
                    await HandleDoorbellTelemetry(doorbellTelemtry);
                    break;
                case AlarmUpdated alarmTelemetry:
                    await HandleAlarmTelemetry(alarmTelemetry);
                    break;
            }
        }

        protected virtual Task HandleLightTelemetry(LightUpdated lightTelemetry)
        {
            return Task.CompletedTask;
        }

        protected virtual Task HandleDoorbellTelemetry(DoorbellUpdated doorbellTelemetry)
        {
            return Task.CompletedTask;
        }

        protected virtual Task HandleAlarmTelemetry(AlarmUpdated alarmTelemetry)
        {
            return Task.CompletedTask;
        }
        protected void SendTelemetry(Telemetry telemetry)
        {
            TelemetryReceived?.Invoke(this, telemetry);
        }
        #endregion

        
        public virtual void Dispose()
        {
        }
    }
}
