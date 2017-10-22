using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Telemetry
{
    public abstract class Telemetry
    {
        public Telemetry()
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTimeOffset.Now;
        }
        public Guid Id { get; }
        public DateTimeOffset OccurredAt { get; set; }
    }
}
