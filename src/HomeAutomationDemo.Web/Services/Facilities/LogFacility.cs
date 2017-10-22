using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using HomeAutomationDemo.Model.Telemetry;

namespace HomeAutomationDemo.Web.Services.Facilities
{
    public class LogFacility : BaseFacility
    {
        private readonly string filename;
        private static object fileLock = new object();
        public LogFacility(IHostingEnvironment env)
        {
            filename = Path.Combine(env.ContentRootPath, "log.txt");
        }

        protected override async Task HandleLightTelemetry(LightUpdated lightTelemetry)
        {
            await Log($"The light in the {lightTelemetry.Light.ToString().ToUpperInvariant()} room is now { lightTelemetry.Status.ToString().ToUpperInvariant()}");
        }

        protected override async Task HandleDoorbellTelemetry(DoorbellUpdated doorbellTelemetry)
        {
            await Log($"The doorbell is now { doorbellTelemetry.Status.ToString().ToUpperInvariant()}");
        }

        protected override async Task HandleAlarmTelemetry(AlarmUpdated alarmTelemetry)
        {
            await Log($"The alarm is now {alarmTelemetry.Status.ToString().ToUpperInvariant()}");
        }
        private async Task Log(string message)
        {
            try
            {
                await File.AppendAllTextAsync(filename, $"{Environment.NewLine}{DateTimeOffset.Now}\t{message}");
            }
            catch { }
        }
    }
}
