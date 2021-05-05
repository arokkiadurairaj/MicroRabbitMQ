using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Event
    {
        public DateTime TimeStamp { get; protected set; }

        public Event()
        {
            this.TimeStamp = DateTime.Now;
        }
    }
}
