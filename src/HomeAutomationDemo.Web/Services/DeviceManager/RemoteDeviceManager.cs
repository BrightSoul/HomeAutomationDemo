using HomeAutomationDemo.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using HomeAutomationDemo.Model.Status;
using HomeAutomationDemo.Model.Enums;
using HomeAutomationDemo.Model.Commands;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Azure.EventHubs;
using HomeAutomationDemo.Model.Telemetry;

namespace HomeAutomationDemo.Web.Services.DeviceManager {
    public class RemoteDeviceManager : IDeviceManager
    {
        private readonly IServiceProvider serviceProvider;
        private readonly List<IDeviceControlFacility> deviceFacilities;
        private readonly ServiceClient serviceClient;
        private const string connectionString = "HostName=homeautomationdemo.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=mpWICmYoSAHnSl9Ed5DVYURAL0yJyM4akoPxk8DXkzE=";
        static string iotHubD2cEndpoint = "messages/events";

        //Statuses
        private readonly Dictionary<Light, LightStatus> lights;
        private DoorbellStatus doorbellStatus = DoorbellStatus.Off;
        private AlarmStatus alarmStatus = AlarmStatus.Off;
        private EventHubClient eventHubClient;
        private CancellationTokenSource receiveToken;

        public RemoteDeviceManager(IServiceProvider serviceProvider)
        {
            deviceFacilities = new List<IDeviceControlFacility>();
            lights = new Dictionary<Light, LightStatus>() { { Light.Blue, LightStatus.Off }, { Light.Green, LightStatus.Off }, { Light.Red, LightStatus.Off }, { Light.Yellow, LightStatus.Off } };
            this.serviceProvider = serviceProvider;
            //TODO: sistema qui
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            UpdateCurrentStatus();

        }


        private async void StartReceivingMessages()
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);

            var d2cPartitions = (await eventHubClient.GetRuntimeInformationAsync()).PartitionIds;

            receiveToken = new CancellationTokenSource();

            foreach (string partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition, receiveToken.Token);
            }
            //Task.WaitAll(tasks.ToArray());

        }

        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.CreateReceiver("$Default", partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                var eventDataCollection = await eventHubReceiver.ReceiveAsync(1, TimeSpan.FromSeconds(2));
                if (eventDataCollection == null) continue;

                foreach (var eventData in eventDataCollection) {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    var telemetria = JsonConvert.DeserializeObject(data);
                    switch (telemetria)
                    {
                        case UpdateAlarm updateAlarmCommand:
                            foreach (var facility in deviceFacilities)
                            {
                                await facility.UpdateAlarm(updateAlarmCommand.DesiredStatus);
                            }
                            break;
                        case UpdateDoorbell updateDoorbellCommand:
                            foreach (var facility in deviceFacilities)
                            {
                                await facility.UpdateDoorbell(updateDoorbellCommand.DesiredStatus);
                            }
                            break;
                        case UpdateLight updateLightCommand:
                            foreach (var facility in deviceFacilities)
                            {
                                await facility.UpdateLight(updateLightCommand.Light, updateLightCommand.DesiredStatus);
                            }
                            break;
                       

                }
                }


               
               
                //TODO: esamina il tipo e cicla
                

            }
        }


        private void UpdateCurrentStatus()
        {
            CurrentStatus = new Model.Status.DeviceStatus(lights, doorbellStatus, alarmStatus);
        }

        public Model.Status.DeviceStatus CurrentStatus { get; set; }

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
                await SendMessage(updateLightCommand);
            }
        }

        private async Task HandleDoorbellCommand(UpdateDoorbell updateDoorbellCommand)
        {
            if (doorbellStatus != updateDoorbellCommand.DesiredStatus)
            {
                await SendMessage(updateDoorbellCommand);
            }
        }

        private async Task HandleUpdateAlarmCommand(UpdateAlarm updateAlarmCommand)
        {
            if (alarmStatus != updateAlarmCommand.DesiredStatus)
            {
                await SendMessage(updateAlarmCommand);
            }
        }

        private async Task SendMessage(Command command)
        {
            await serviceClient.OpenAsync();
            var serialized = JsonConvert.SerializeObject(command, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            var data = new Message(Encoding.UTF8.GetBytes(serialized));
            await serviceClient.SendAsync("home1", data);
            await serviceClient.CloseAsync();
        }
        #endregion

        public void Dispose()
        {
            foreach (var facility in deviceFacilities)
            {
                facility.CommandReceived -= HandleDeviceCommand;
                facility.Dispose();
            }
            receiveToken.Cancel();
        }


    }
}