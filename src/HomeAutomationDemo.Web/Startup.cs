using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HomeAutomationDemo.Web.Services.DeviceControlFacilities;
using HomeAutomationDemo.Web.Services;
using HomeAutomationDemo.Web.Services.DeviceManager;

namespace HomeAutomationDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        private static IDeviceManager remoteControlService;
        public void ConfigureServices(IServiceCollection services)
        {

            //#if !DEBUG
            services.AddSingleton<IDeviceControlFacility, GpioFacility>();
            //#endif

            services.AddSingleton<IDeviceControlFacility, AzureIotHubFacility>();
            services.AddSingleton<IDeviceControlFacility, LogFacility>();
            services.AddSingleton<IDeviceControlFacility, WebsocketFacility>();
            services.AddSingleton<IDeviceControlFacility, ConsoleFacility>();

            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<IDeviceStatusProvider>(serviceProvider => serviceProvider.GetService<IDeviceManager>());
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            applicationLifetime.ApplicationStarted.Register(() => OnStart(serviceProvider));
            applicationLifetime.ApplicationStopped.Register(OnStop);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseStaticFiles();
            app.UseDefaultFiles();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
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
