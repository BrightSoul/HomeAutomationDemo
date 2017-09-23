using System;
using Microsoft.AspNetCore.Builder;

namespace HomeAutomationDemo.Extensions {
    public static class AppExtensions {
        public static void UseRemoteControl(this IApplicationBuilder app) {
             var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
            //app.UseMiddleware<RemoteControlMiddleware>();
        }
    }
}