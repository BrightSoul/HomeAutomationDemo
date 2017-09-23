using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Telemetry
{
    public class DoorbellUpdated : TelemetryEvent
    {
        public DoorbellStatus Status { get; set; }
    }
}
