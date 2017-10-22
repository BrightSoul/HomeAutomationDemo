using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Services
{
    public interface IWebSocketControlFacility
    {
        Task HandleWebSocketCommunication(HttpContext httpContext, WebSocket webSocket);
    }
}
