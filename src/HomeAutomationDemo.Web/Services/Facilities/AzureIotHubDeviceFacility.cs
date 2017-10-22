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
using System.Threading;
using System.IO;

namespace HomeAutomationDemo.Web.Services.Facilities
{
    public class AzureIotHubDeviceFacility : BaseFacility
    {

        private readonly IDeviceStatusProvider deviceStatusProvider;
        private DeviceClient deviceClient = null;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly AppConfig config;

        public AzureIotHubDeviceFacility(IDeviceStatusProvider deviceStatusProvider, AppConfig config)
        {
            this.deviceStatusProvider = deviceStatusProvider;
            cancellationTokenSource = new CancellationTokenSource();
            this.config = config;
            ReceiveCommands();
        }
        
        public override async Task HandleTelemetry(Telemetry telemetry)
        {
            try
            {
                await EnsureDeviceClientIsReady();

                var payload = JsonConvert.SerializeObject(telemetry, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                var message = new Message(Encoding.UTF8.GetBytes(payload));
                await deviceClient.SendEventAsync(message);
            }
            catch (Exception exc)
            {
                //TODO: exceptions themselves should be treated as telemetry
                //We just write to console instead in this demo
                Console.WriteLine($"Error sending a telemetry message: {exc.Message}");
            }
        }

        //Warning: this is NOT production code. The device client connection must be checked and healed 
        //in case it goes down due to a transient connectivity problem
        private async void ReceiveCommands()
        {
            await EnsureDeviceClientIsReady();
            var timeout = TimeSpan.FromSeconds(2);
            Message message;
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                message = await deviceClient.ReceiveAsync(timeout);
                if (message == null)
                    continue;

                using (var streamReader = new StreamReader(message.BodyStream)) {
                    var jsonContent = await streamReader.ReadToEndAsync();
                    var command = JsonConvert.DeserializeObject(jsonContent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }) as Command;
                    if (command != null)
                    {
                        SendCommand(command);
                        //Tell the IoT Hub the message has been processed
                        await deviceClient.CompleteAsync(message);
                    } else
                    {
                        //Tell the IoT Hub to discard the message since we haven't understood it
                        await deviceClient.RejectAsync(message);
                    }
                }
            }
        }

        private async Task EnsureDeviceClientIsReady()
        {
            if (deviceClient == null)
            {
                deviceClient = DeviceClient.CreateFromConnectionString(config.IotHubDeviceConnectionString);
                await deviceClient.OpenAsync();
                await deviceClient.SetMethodHandlerAsync("GetCurrentStatus", SendCurrentStatus, null);
            }
        }

        private Task<MethodResponse> SendCurrentStatus(MethodRequest methodRequest, object userContext)
        {
            var jsonContent = JsonConvert.SerializeObject(deviceStatusProvider.CurrentStatus, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(jsonContent), 0));
        }

        public override void Dispose()
        {
            cancellationTokenSource.Cancel();
            if (deviceClient != null)
            {
                deviceClient.CloseAsync().Wait();
            }
        }
    }
}
