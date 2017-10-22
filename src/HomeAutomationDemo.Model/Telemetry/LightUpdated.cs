using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Telemetry
{
    public class LightUpdated : Telemetry
    {
        public Light Light { get; set; }
        public LightStatus Status { get; set; }
    }
}
