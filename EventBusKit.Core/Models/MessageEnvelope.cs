using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusKit.Core.Models
{
    /// <summary>
    /// Wrapper for any message payload with its context.
    /// </summary>
    public class MessageEnvelope<T>
    {
        public T Payload { get; set; }
        public MessageContext Context { get; set; }

        public MessageEnvelope(T payload, MessageContext context)
        {
            Payload = payload;
            Context = context;
        }
    }
}
