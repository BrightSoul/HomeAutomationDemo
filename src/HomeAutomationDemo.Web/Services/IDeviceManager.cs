using HomeAutomationDemo.Model.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Services
{
    public interface IDeviceManager : IDeviceStatusProvider, IDisposable
    {
        void Init();
    }
}
