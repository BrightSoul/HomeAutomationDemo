using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Telemetry
{
    public class DoorbellUpdated : Telemetry
    {
        public DoorbellStatus Status { get; set; }
    }
}
