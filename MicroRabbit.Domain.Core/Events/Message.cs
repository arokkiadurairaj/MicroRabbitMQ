using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Message : IRequest<bool>
    {
        public string MessageType { get; protected set; }

        public Message()
        {
            this.MessageType = this.GetType().Name;
        }
    }
}
