using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Telemetry;
using HomeAutomationDemo.Model.Enums;
using System.Net.Mail;
using System.Net;

namespace HomeAutomationDemo.Web.Services.Facilities
{
    public class AlertFacility : BaseFacility
    {
        private readonly IDeviceStatusProvider deviceStatusProvider;
        private readonly AppConfig config;
        public AlertFacility(IDeviceStatusProvider deviceStatusProvider, AppConfig config)
        {
            this.config = config;
            this.deviceStatusProvider = deviceStatusProvider;
        }
        protected override async Task HandleAlarmTelemetry(AlarmUpdated alarmTelemetry)
        {
            if (alarmTelemetry.Status == AlarmStatus.Active && deviceStatusProvider.CurrentStatus.Alarm != AlarmStatus.Active)
            {
                //Someone broke into our house!!
                await SendAlertMail();
            } else if (alarmTelemetry.Status != AlarmStatus.Active && deviceStatusProvider.CurrentStatus.Alarm == AlarmStatus.Active)
            {
                //The alarm was turned off
                await SendRecoveryMail();
            }
        }

        private async Task SendAlertMail()
        {
            await SendMail("Allarme intruso!", "Il portone d'ingresso si è attivato mentre l'allarme era attivo");
        }

        private async Task SendRecoveryMail()
        {
            await SendMail("Allarme rientrato", "L'allarme del portone d'ingresso è stato disattivato");
        }

        private async Task SendMail(string subject, string body)
        {
            using (var client = new SmtpClient
            {
                Host = config.SmtpHost,
                Port = config.SmtpPort,
                EnableSsl = config.SmtpEnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword)
            })
            {
                try
                {
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(config.EmailAddress);
                    mailMessage.To.Add(config.EmailAddress);
                    mailMessage.Body = body;
                    mailMessage.Subject = subject;
                    await client.SendMailAsync(mailMessage);
                }
                catch
                {

                }
            }
        }
    }
}
