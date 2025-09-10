using EventBusKit.Core.Models;
using System;
using System.Threading.Tasks;

namespace EventBusKit.Core.Abstractions
{
    /// <summary>
    /// Defines the contract for publishing messages to a message bus.
    /// Supports publishing raw payloads, CloudEvents, and message envelopes.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a raw payload as a CloudEvent to the specified topic.
        /// The publisher will wrap the payload in a CloudEvent automatically.
        /// </summary>
        /// <typeparam name="T">Type of the payload data.</typeparam>
        /// <param name="topic">The topic or subject to publish to.</param>
        /// <param name="payload">The payload data to publish.</param>
        /// <param name="source">Optional source identifier of the event.</param>
        /// <param name="type">Optional type of the event (e.g., com.example.event.created).</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task PublishAsync<T>(string topic, T payload, string? source = null, string? type = null);

        /// <summary>
        /// Publishes a CloudEvent to the specified topic.
        /// </summary>
        /// <typeparam name="T">Type of the CloudEvent payload.</typeparam>
        /// <param name="topic">The topic or subject to publish to.</param>
        /// <param name="cloudEvent">The CloudEvent object to publish.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task PublishAsync<T>(string topic, CloudEvent<T> cloudEvent);

        /// <summary>
        /// Publishes a MessageEnvelope containing a CloudEvent and metadata to the specified topic.
        /// This method is used when additional message context is required (e.g., headers, correlation IDs).
        /// </summary>
        /// <typeparam name="T">Type of the CloudEvent payload inside the envelope.</typeparam>
        /// <param name="topic">The topic or subject to publish to.</param>
        /// <param name="envelope">The MessageEnvelope containing payload and context.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task PublishAsync<T>(string topic, MessageEnvelope<CloudEvent<T>> envelope);

        /// <summary>
        /// Publishes a message with a specified delivery mode (AtMostOnce, AtLeastOnce, ExactlyOnce).
        /// Useful for configuring message reliability and guarantees.
        /// </summary>
        /// <typeparam name="T">Type of the payload data.</typeparam>
        /// <param name="topic">The topic or subject to publish to.</param>
        /// <param name="payload">The payload data to publish.</param>
        /// <param name="deliveryMode">The delivery mode for the message.</param>
        /// <param name="source">Optional source identifier of the event.</param>
        /// <param name="type">Optional type of the event.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task PublishAsync<T>(string topic, T payload, DeliveryMode deliveryMode, string? source = null, string? type = null);
    }
}
