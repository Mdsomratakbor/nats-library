using System;
using System.Threading;
using System.Threading.Tasks;
using NatsLibrary.Core.Configurations;

namespace NatsLibrary.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for subscribing to NATS subjects.
    /// Supports both Core NATS and JetStream subscriptions.
    /// </summary>
    public interface INatsSubscriber
    {
        /// <summary>
        /// Subscribe to a subject with a handler.
        /// Suitable for simple Core NATS subscriptions with optional queue groups.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the message payload. 
        /// The payload will be deserialized into this type.
        /// </typeparam>
        /// <param name="subject">
        /// The NATS subject (topic) to subscribe to.
        /// </param>
        /// <param name="handler">
        /// A function that processes incoming messages.
        /// Invoked for every received message of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="queueGroup">
        /// (Optional) Queue group name.  
        /// - When specified, subscribers with the same queue group share messages (load balancing).  
        /// - If null, all subscribers will receive all messages.
        /// </param>
        /// <param name="cancellationToken">
        /// (Optional) A token to cancel the subscription task.
        /// </param>
        Task SubscribeAsync<T>(
            string subject,
            Func<T, Task> handler,
            string? queueGroup = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Subscribe to a subject with advanced subscriber options.
        /// Supports Core NATS and JetStream features (ack policies, replay, filters, etc.).
        /// </summary>
        /// <typeparam name="T">
        /// The type of the message payload. 
        /// The payload will be deserialized into this type.
        /// </typeparam>
        /// <param name="subject">
        /// The NATS subject (topic) to subscribe to.
        /// </param>
        /// <param name="handler">
        /// A function that processes incoming messages.
        /// Invoked for every received message of type <typeparamref name="T"/>.
        /// </param>
        /// <param name="options">
        /// (Optional) Subscription configuration options.  
        /// Includes durable name, acknowledgment policy, replay policy, filter subjects, etc.  
        /// If null, default subscriber settings will be applied.
        /// </param>
        /// <param name="cancellationToken">
        /// (Optional) A token to cancel the subscription task.
        /// </param>
        Task SubscribeAsync<T>(
            string subject,
            Func<T, Task> handler,
            SubscriberOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}
