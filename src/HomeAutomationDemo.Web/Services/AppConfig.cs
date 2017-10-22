using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Services
{
    public class AppConfig
    {
        public AppConfig(IConfiguration configuration)
        {
            configuration.Bind(this);
        }

        public string IotHubDeviceConnectionString { get; set; }
        public string IotHubDeviceId { get; set; }
        public string IotHubServiceConnectionString { get; set; }
        public string EventHubConnectionString { get; set; }
        public string EventHubPath { get; set; }
        public string StorageConnectionString { get; set; }
        public string StorageContainerName { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string EmailAddress { get; set; }
    }
}
