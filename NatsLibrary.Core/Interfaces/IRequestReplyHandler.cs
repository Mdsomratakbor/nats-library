using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Interfaces
{
    public interface IRequestReplyHandler
    {
        /// <summary>
        /// Send a request and await a response
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="subject">Subject to send request to</param>
        /// <param name="request">Request object</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        Task<TResponse?> RequestAsync<TRequest, TResponse>(string subject, TRequest request, int timeoutMs = 5000);

        /// <summary>
        /// Handle incoming requests on a subject
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="subject">Subject to listen for requests</param>
        /// <param name="handler">Handler to process requests</param>
        Task ReplyAsync<TRequest, TResponse>(string subject, Func<TRequest, Task<TResponse>> handler);
    }
}
