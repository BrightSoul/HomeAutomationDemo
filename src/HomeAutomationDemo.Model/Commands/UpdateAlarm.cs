using HomeAutomationDemo.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Commands
{
    public class UpdateAlarm : Command
    {
        public AlarmStatus DesiredStatus { get; set; }
    }
}
