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

namespace HomeAutomationDemo.Web.Services.DeviceControlFacilities
{
    public class WebsocketFacility : IDeviceControlFacility, IWebSocketControlFacility
    {
        public event EventHandler<Command> CommandReceived;
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
                        HandleLightCommand(input);
                        break;
                    case "alarm":
                        HandleAlarmCommand(input);
                        break;
                    case "doorbell":
                        await HandleDoorbellCommand(input);
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
                message = UpdateLightMessage(light.Key, light.Value);
                await SendMessage(message, webSocket);
            }

            message = UpdateAlarmMessage(status.Alarm);
            await SendMessage(message, webSocket);

            message = UpdateDoorbellMessage(status.Doorbell);
            await SendMessage(message, webSocket);
        }

        private async Task SendMessage(string message, WebSocket webSocket = null)
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

        private async Task HandleDoorbellCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 1)
            {
                return;
            }

            CommandReceived?.Invoke(this, new UpdateDoorbell { DesiredStatus = DoorbellStatus.On });
            await Task.Delay(1000);
            CommandReceived?.Invoke(this, new UpdateDoorbell { DesiredStatus = DoorbellStatus.Off });
        }

        private void HandleAlarmCommand(string[] commandArguments)
        {
            if (commandArguments.Length != 2)
            {
                return;
            }

            if (!Enum.TryParse(commandArguments[1], true, out AlarmStatus alarmStatus))
            {
                return;
            }

            CommandReceived?.Invoke(this, new UpdateAlarm { DesiredStatus = alarmStatus });
        }

        private void HandleLightCommand(string[] commandArguments)
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

            CommandReceived?.Invoke(this, new UpdateLight { Light = light, DesiredStatus = lightStatus });
        }


        public async Task UpdateAlarm(AlarmStatus status)
        {
            var message = UpdateAlarmMessage(status);
            await SendMessage(message);
        }

        private string UpdateAlarmMessage(AlarmStatus status)
        {
            return $"alarm {status.ToString().ToLower()}";
        }

        public async Task UpdateLight(Light light, LightStatus status)
        {
            var message = UpdateLightMessage(light, status);
            await SendMessage(message);
        }

        private string UpdateLightMessage(Light light, LightStatus status)
        {
            return $"light {status.ToString().ToLower()} {light.ToString().ToLower()}";
        }

        public async Task UpdateDoorbell(DoorbellStatus status)
        {
            var message = UpdateDoorbellMessage(status);
            await SendMessage(message);
        }

        private string UpdateDoorbellMessage(DoorbellStatus status)
        {
            return $"doorbell {status.ToString().ToLower()}";
        }

        public void Dispose()
        {
            
        }
    }
}
