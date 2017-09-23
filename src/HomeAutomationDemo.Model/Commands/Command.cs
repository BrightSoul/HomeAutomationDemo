using System;
using System.Collections.Generic;
using System.Text;

namespace HomeAutomationDemo.Model.Commands
{
    public abstract class Command
    {
        public Command()
        {
            Id = Guid.NewGuid();
            SentAt = DateTimeOffset.Now;
        }
        public Guid Id { get; }
        public DateTimeOffset SentAt { get; set; }
    }
}
