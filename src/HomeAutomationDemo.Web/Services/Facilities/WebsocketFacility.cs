using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using HomeAutomationDemo.Web.Extensions;
using System.Collections.Concurrent;
using HomeAutomationDemo.Model.Telemetry;

namespace HomeAutomationDemo.Web.Services.Facilities
{
    public class WebsocketFacility : BaseFacility, IWebSocketControlFacility
    {
        private readonly ConcurrentDictionary<WebSocket, Guid> clients;

        private readonly IDeviceStatusProvider deviceStatusProvider;

        public WebsocketFacility(IDeviceStatusProvider deviceStatusProvider)
        {
            //this.nextMiddleware = nextMiddleware;
            this.clients = new ConcurrentDictionary<WebSocket, Guid>();
            this.deviceStatusProvider = deviceStatusProvider;
        }

        public async Task Invoke(HttpContext httpContext, RequestDelegate nextMiddleware)
        {
            if (httpContext.Request.Protocol != "ws")
            {
                await nextMiddleware(httpContext);
                return;
            }

            WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketCommunication(httpContext, webSocket);
        }

        public async Task HandleWebSocketCommunication(HttpContext context, WebSocket webSocket)
        {
            clients.TryAdd(webSocket, Guid.NewGuid());

            var buffer = new byte[100];
            await SendStatus(webSocket);
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var input = Encoding.UTF8.GetString(buffer).Trim(' ', '\0').AsCommandArguments();
                var commandName = input.FirstOrDefault()?.ToLowerInvariant();
                switch (commandName)
                {
                    case "light":
                        SendLightCommand(input);
                        break;
                    case "alarm":
                        SendAlarmCommand(input);
                        break;
                    case "doorbell":
                        await SendDoorbellCommand(input);
                        break;
                }
                buffer = new byte[100];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            clients.TryRemove(webSocket, out Guid connectionId);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

        }

        private async Task SendStatus(WebSocket webSocket)
        {
            var status = deviceStatusProvider.CurrentStatus;
            string message;
            foreach (var light in status.Lights)
            {
                message = CreateLightNotification(light.Key, light.Value);
                await SendNotificationToClients(message, webSocket);
            }

            message = CreateAlarmNotification(status.Alarm);
            await SendNotificationToClients(message, webSocket);

            message = CreateDoorbellNotification(status.Doorbell);
            await SendNotificationToClients(message, webSocket);
        }

        private async Task SendNotificationToClients(string message, WebSocket webSocket = null)
        {
            var sockets = webSocket != null ? new[] { webSocket } : clients.Keys.ToArray();
            foreach (var socket in sockets)
            {
                if (socket.CloseStatus.HasValue)
                {
                    clients.TryRemove(socket, out Guid connectionId);
                    continue;
                }

                var responseBuffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(responseBuffer, 0, responseBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task SendDoorbellCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 1)
            {
                return;
            }

            SendCommand(new UpdateDoorbell { DesiredStatus = DoorbellStatus.On });
            await Task.Delay(1000);
            SendCommand(new UpdateDoorbell { DesiredStatus = DoorbellStatus.Off });
        }

        private void SendAlarmCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 2)
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[1], true, out AlarmStatus alarmStatus))
            {
                return;
            }

            SendCommand(new UpdateAlarm { DesiredStatus = alarmStatus });
        }

        private void SendLightCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 3)
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[1], true, out LightStatus lightStatus))
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[2], true, out Light light))
            {
                return;
            }

            SendCommand(new UpdateLight { Light = light, DesiredStatus = lightStatus });
        }

        protected override async Task HandleAlarmTelemetry(AlarmUpdated alarmTelemetry)
        {
            var notification = CreateAlarmNotification(alarmTelemetry.Status);
            await SendNotificationToClients(notification);
        }

        private string CreateAlarmNotification(AlarmStatus status)
        {
            return $"alarm {status.ToString().ToLower()}";
        }

        protected override async Task HandleLightTelemetry(LightUpdated lightTelemetry)
        {
            var notification = CreateLightNotification(lightTelemetry.Light, lightTelemetry.Status);
            await SendNotificationToClients(notification);
        }

        private string CreateLightNotification(Light light, LightStatus status)
        {
            return $"light {status.ToString().ToLower()} {light.ToString().ToLower()}";
        }

        protected override async Task HandleDoorbellTelemetry(DoorbellUpdated doorbellTelemetry)
        {
            var notification = CreateDoorbellNotification(doorbellTelemetry.Status);
            await SendNotificationToClients(notification);
        }

        private string CreateDoorbellNotification(DoorbellStatus status)
        {
            return $"doorbell {status.ToString().ToLower()}";
        }

    }
}
