using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Telemetry
{
    public class AlarmUpdated : Telemetry
    {
        public AlarmStatus Status { get; set; }
    }
}
