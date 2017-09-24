using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using HomeAutomationDemo.Model.Telemetry;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class AzureIotHubFacility : IDeviceControlFacility
    {

        private readonly IDeviceStatusProvider deviceStatusProvider;
        private const string connectionString = "HostName=homeautomationdemo.azure-devices.net;DeviceId=home1;SharedAccessKey=69Aml21vFuonQ4Sd4DKzVw531oRw9xQwlZdeTSPa2jA=";
        private DeviceClient deviceClient = null;

        public AzureIotHubFacility(IDeviceStatusProvider deviceStatusProvider)
        {
            this.deviceStatusProvider = deviceStatusProvider;
        }

        public event EventHandler<Command> CommandReceived;

        private async Task SendTelemetry(TelemetryEvent telementryEvent)
        {
            try
            {
                if (deviceClient == null)
                {
                    deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
                    await deviceClient.OpenAsync();
                }

                var payload = JsonConvert.SerializeObject(telementryEvent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                var message = new Message(Encoding.UTF8.GetBytes(payload));
                await deviceClient.SendEventAsync(message);
            } catch (Exception exc)
            {
                Console.WriteLine($"Error sending a telemetry message: {exc.Message}");
            }
        }

        public async Task UpdateAlarm(AlarmStatus status)
        {
            await SendTelemetry(new AlarmUpdated { Status = status });
        }

        public async Task UpdateLight(Light light, LightStatus status)
        {
            await SendTelemetry(new LightUpdated { Light = light, Status = status });
        }

        public async Task UpdateDoorbell(DoorbellStatus status)
        {
            await SendTelemetry(new DoorbellUpdated { Status = status });
        }

        public void Dispose()
        {
            if (deviceClient != null)
            {
                deviceClient.CloseAsync().Wait();
            }
        }
    }
}
