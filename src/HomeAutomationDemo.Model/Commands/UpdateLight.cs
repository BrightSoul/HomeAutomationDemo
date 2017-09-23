using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Commands
{
    public class UpdateLight : Command
    {
        public Light Light { get; set; }
        public LightStatus DesiredStatus { get; set; }
    }
}
