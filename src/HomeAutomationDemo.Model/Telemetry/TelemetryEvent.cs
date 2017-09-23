using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Telemetry
{
    public abstract class TelemetryEvent
    {
        public TelemetryEvent()
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTimeOffset.Now;
        }
        public Guid Id { get; }
        public DateTimeOffset OccurredAt { get; set; }
    }
}
