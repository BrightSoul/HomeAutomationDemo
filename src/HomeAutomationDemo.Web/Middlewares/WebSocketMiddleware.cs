using HomeAutomationDemo.Web.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Middlewares
{
    public class WebSocketMiddleware
    {

        private readonly RequestDelegate nextMiddleware;
        private readonly IWebSocketControlFacility webSocketFacility;
        public WebSocketMiddleware(RequestDelegate nextMiddleware, IWebSocketControlFacility webSocketFacility)
        {
            this.webSocketFacility = webSocketFacility;
            this.nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.WebSockets.IsWebSocketRequest)
            {
                await nextMiddleware(httpContext);
                return;
            }

            WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await webSocketFacility.HandleWebSocketCommunication(httpContext, webSocket);
        }
    }
}
