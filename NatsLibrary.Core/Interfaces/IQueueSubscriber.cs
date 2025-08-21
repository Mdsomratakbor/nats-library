using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Interfaces
{
    public interface IQueueSubscriber
    {
        /// <summary>
        /// Subscribe to a subject using a queue group for load-balancing
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="subject">Subject name</param>
        /// <param name="queueGroup">Queue group name</param>
        /// <param name="handler">Handler invoked on each message</param>
        Task SubscribeQueueAsync<T>(string subject, string queueGroup, Func<T, Task> handler);
    }
}
