using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HomeAutomationDemo.Service;
using Microsoft.AspNetCore.Http;
namespace HomeAutomationDemo.Middlewares {
    public class RemoteControlMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IRemoteControlService remoteControlService;

        public RemoteControlMiddleware(RequestDelegate next, IRemoteControlService remoteControlService)
        {
            this.next = next;
            this.remoteControlService = remoteControlService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Protocol != "ws") {
                await next(httpContext);
                return;
            }

            WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketConnection(httpContext, webSocket);
        }

        private async Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        string content = Encoding.UTF8.GetString(buffer).Trim();
        
        string response = "";
        switch (content.Split(':')[0]) {
            case "LIGHT":
                response = remoteControlService.UpdateLight(content);
                break;
            case "ALARM":
                response = remoteControlService.UpdateAlarm(content);
                break;
        }
        var responseBuffer = Encoding.UTF8.GetBytes(response);
        await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer, 0, responseBuffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

    }
}