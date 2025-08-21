using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Interfaces
{
    public interface INatsSubscriber
    {
        /// <summary>
        /// Subscribe to a subject
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="subject">Subject name</param>
        /// <param name="handler">Handler invoked on each message</param>
        /// <param name="queueGroup">Optional queue group for load balancing</param>
        /// 
        Task SubscribeAsync<T>(string subject, Func<T, Task> handler, string? queueGroup = null, CancellationToken cancellationToken = default);
    }
}
