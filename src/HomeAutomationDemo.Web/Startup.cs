using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HomeAutomationDemo.Web.Services.Facilities;
using HomeAutomationDemo.Web.Services;
using HomeAutomationDemo.Web.Services.DeviceManager;
using HomeAutomationDemo.Web.Middlewares;
using Unosquare.RaspberryIO;

namespace HomeAutomationDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        private static IDeviceManager remoteControlService;
        public void ConfigureServices(IServiceCollection services)
        {

            //Let's see if we're installed locally by attempting to retrieve the Raspberry PI info
            try
            {
                var info = Pi.Info;
                //yes, we can turn GPIO's on and off
                services.AddSingleton<BaseFacility, GpioFacility>();
                services.AddSingleton<BaseFacility, AzureIotHubDeviceFacility>();
                Console.WriteLine("Starting in LOCAL mode");
            } catch
            {
                //no, this webapp is installed on a remote machine so we rely on IoT Hubs to deliver commands
                services.AddSingleton<BaseFacility, AlertFacility>();
                services.AddSingleton<BaseFacility, AzureIotHubServiceFacility>();
                Console.WriteLine("Starting in REMOTE mode");
            }
            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<BaseFacility, LogFacility>();
            services.AddSingleton<BaseFacility, WebsocketFacility>();
            services.AddSingleton<BaseFacility, ConsoleFacility>();
            services.AddSingleton<AppConfig>();

            
            services.AddSingleton<IDeviceStatusProvider>(serviceProvider => serviceProvider.GetService<IDeviceManager>());
            services.AddSingleton<IWebSocketControlFacility>(serviceProvider => serviceProvider.GetServices<BaseFacility>().OfType<IWebSocketControlFacility>().First());
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Environment: {env.EnvironmentName}");
            applicationLifetime.ApplicationStarted.Register(() => OnStart(serviceProvider));
            applicationLifetime.ApplicationStopped.Register(OnStop);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebSocketMiddleware>();

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }


        private void OnStart(IServiceProvider serviceProvider)
        {
            remoteControlService = serviceProvider.GetService<IDeviceManager>();
            remoteControlService.Init();
        }

        private void OnStop()
        {
            if (remoteControlService != null)
                remoteControlService.Dispose();
        }


    }
}
