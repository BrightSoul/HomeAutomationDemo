namespace HomeAutomationDemo.Service {
    public class RemoteControlService : IRemoteControlService
    {
        public RemoteControlService()
        {
        }

        public bool UpdateAlarm(bool desiredStatus)
        {
            return true;
        }

        public bool UpdateLight(int lightId, bool desiredStatus)
        {
            return true;
        }
    }
}