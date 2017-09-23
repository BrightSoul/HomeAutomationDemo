using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class LogFacility : IDeviceControlFacility
    {
        private readonly string filename;
        private static object fileLock = new object();
        public LogFacility(IHostingEnvironment env)
        {
            filename = Path.Combine(env.ContentRootPath, "log.txt");
        }
        public event EventHandler<Command> CommandReceived;

        private async Task Log(string message)
        {
            try
            {
                await File.AppendAllTextAsync(filename, $"{Environment.NewLine}{DateTimeOffset.Now}\t{message}");
            } catch { }
        }

        public async Task UpdateAlarm(AlarmStatus status)
        {
            await Log($"The alarm is now {status.ToString().ToUpperInvariant()}");
        }

        public async Task UpdateLight(Light light, LightStatus status)
        {
            await Log($"The light in the {light.ToString().ToUpperInvariant()} room is now { status.ToString().ToUpperInvariant()}");
        }

        public async Task UpdateDoorbell(DoorbellStatus status)
        {
            await Log($"The doorbel is now { status.ToString().ToUpperInvariant()}");
        }

        public void Dispose()
        {
        }
    }
}
