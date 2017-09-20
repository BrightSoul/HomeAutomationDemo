namespace HomeAutomationDemo.Service
{
    public interface IRemoteControlService
    {
        string UpdateLight(string request);
        string UpdateAlarm(string request);
    }
}