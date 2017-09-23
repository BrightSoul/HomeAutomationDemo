using HomeAutomationDemo.Model.Commands;
using HomeAutomationDemo.Model.Enums;
using System;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Services
{
    public interface IDeviceControlFacility : IDisposable
    {
        Task UpdateLight(Light light, LightStatus status);
        Task UpdateAlarm(AlarmStatus status);
        Task UpdateDoorbell(DoorbellStatus status);

        event EventHandler<Command> CommandReceived;
    }
}