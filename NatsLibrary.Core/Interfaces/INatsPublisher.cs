using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Interfaces
{
    public interface INatsPublisher
    {
        /// <summary>
        /// Publish a message to a subject (NATS or JetStream)
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="subject">Subject name</param>
        /// <param name="message">Message object</param>
        Task PublishAsync<T>(string subject, T message);
    }
}
