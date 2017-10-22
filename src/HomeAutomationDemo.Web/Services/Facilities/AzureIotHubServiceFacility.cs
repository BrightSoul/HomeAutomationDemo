using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using HomeAutomationDemo.Model.Telemetry;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.Devices;
using System.Threading;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace HomeAutomationDemo.Web.Services.Facilities
{
    public class AzureIotHubServiceFacility : BaseFacility, IEventProcessor, IEventProcessorFactory
    {

        private ServiceClient serviceClient;
        private readonly EventProcessorHost processorHost;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly AppConfig config;

        public AzureIotHubServiceFacility(AppConfig config)
        {
            cancellationTokenSource = new CancellationTokenSource();
            this.config = config;

            string eventProcessorHostName = Guid.NewGuid().ToString();
            processorHost = new EventProcessorHost(
                eventProcessorHostName,
                config.EventHubPath,
                PartitionReceiver.DefaultConsumerGroupName,
                config.EventHubConnectionString,
                config.StorageConnectionString,
                config.StorageContainerName);
            processorHost.RegisterEventProcessorFactoryAsync(this, new EventProcessorOptions { InitialOffsetProvider = s => null });
            EnsureServiceClientIsReady();
        }

        public override async Task HandleCommand(Command command)
        {
            try
            {
                await EnsureServiceClientIsReady();

                var payload = JsonConvert.SerializeObject(command, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                var message = new Message(Encoding.UTF8.GetBytes(payload));
                //For this demo we just send commands to one device: home1
                await serviceClient.SendAsync(config.IotHubDeviceId, message);
            }
            catch (Exception exc)
            {
                //TODO: exceptions themselves should be treated as telemetry
                //We just write to console instead in this demo
                Console.WriteLine($"Error sending a telemetry message: {exc.Message}");
            }
        }

        private async Task EnsureServiceClientIsReady()
        {
            if (serviceClient == null)
            {
                serviceClient = ServiceClient.CreateFromConnectionString(config.IotHubServiceConnectionString);
                await serviceClient.OpenAsync();

                //Get the current status using a device method
                var method = new CloudToDeviceMethod("GetCurrentStatus", TimeSpan.FromSeconds(5));
                var result = await serviceClient.InvokeDeviceMethodAsync(config.IotHubDeviceId, method);
                if (result != null && result.Status == 0)
                {
                    SendTelemetryFromResponse(result.GetPayloadAsJson());
                }
            }
        }

        private void SendTelemetryFromResponse(string jsonContent)
        {
            var status = JsonConvert.DeserializeObject(jsonContent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }) as Model.Status.DeviceStatus;
            if (status == null)
                return;

            SendTelemetry(new DoorbellUpdated { Status = status.Doorbell });
            SendTelemetry(new AlarmUpdated { Status = status.Alarm });
            foreach (var light in status.Lights)
            {
                SendTelemetry(new LightUpdated { Light = light.Key, Status = light.Value });
            }
        }

        #region IEventProcessor implementation

        public Task OpenAsync(PartitionContext context)
        {
            return Task.CompletedTask;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            if (messages != null)
            {
                foreach (var eventData in messages)
                {
                    var jsonContent = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    var telemetry = JsonConvert.DeserializeObject(jsonContent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }) as Telemetry;
                    if (telemetry != null)
                    {
                        SendTelemetry(telemetry);
                    }
                }
            }
            return context.CheckpointAsync();
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region IEventProcessorFactory implementation
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return this;
        }
        #endregion
    }
}
