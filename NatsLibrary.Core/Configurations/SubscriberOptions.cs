using NATS.Client.JetStream;

namespace NatsLibrary.Core.Configurations;

/// <summary>
/// Defines the different subscriber patterns supported by NATS and JetStream.
/// Determines how messages are consumed from subjects or streams.
/// </summary>
public enum SubscriberPattern
{
    /// <summary>
    /// Core NATS synchronous subscriber.
    /// - Uses <c>SubscribeSync()</c> and <c>NextMessage()</c>.
    /// - Requires manual polling or a background loop.
    /// - Simple but blocks the thread while waiting for messages.
    /// </summary>
    CoreSync,

    /// <summary>
    /// Core NATS asynchronous subscriber.
    /// - Uses <c>SubscribeAsync()</c>.
    /// - Messages are pushed via event callbacks.
    /// - Recommended for most scenarios (non-blocking).
    /// </summary>
    CoreAsync,

    /// <summary>
    /// Core NATS queue subscriber (work queue).
    /// - Multiple subscribers can join the same queue group.
    /// - Each message is delivered to exactly one member of the group.
    /// - Provides load balancing between workers.
    /// </summary>
    CoreQueue,

    /// <summary>
    /// JetStream push subscriber.
    /// - Messages are pushed from the server to the client asynchronously.
    /// - Supports consumer configuration (durable, ack policy, filters, etc.).
    /// </summary>
    JetStreamPush,

    /// <summary>
    /// JetStream pull subscriber.
    /// - Client explicitly fetches messages using <c>Fetch()</c>.
    /// - Provides backpressure control.
    /// - Useful for batch processing.
    /// </summary>
    JetStreamPull,

    /// <summary>
    /// JetStream queue subscriber.
    /// - Similar to <see cref="CoreQueue"/> but for JetStream.
    /// - Multiple consumers share the load of message delivery.
    /// - Useful for scaling with JetStream durability.
    /// </summary>
    JetStreamQueue,

    /// <summary>
    /// JetStream ordered subscriber.
    /// - Ensures messages are received in strict order.
    /// - Automatically handles sequence mismatches by recreating the consumer.
    /// - Good for use cases where ordering is critical (e.g., event sourcing).
    /// </summary>
    JetStreamOrdered
}

/// <summary>
/// Configuration options for a subscriber in NATS or JetStream.
/// Controls subscription mode, acknowledgment behavior, delivery policies,
/// backpressure settings, and reliability features.
/// </summary>
public class SubscriberOptions
{
    /// <summary>
    /// Defines the subscriber pattern (Core NATS, JetStream, Queue, etc.).
    /// Determines how messages will be consumed.
    /// Default = <see cref="SubscriberPattern.CoreAsync"/>.
    /// </summary>
    public SubscriberPattern Pattern { get; set; } = SubscriberPattern.CoreAsync;

    /// <summary>
    /// Whether to run the subscriber in a background task.
    /// - Relevant only for synchronous subscribers (<c>CoreSync</c>, <c>JetStreamPull</c>).
    /// - If true, messages are consumed continuously in a background loop.
    /// - If false, the user must call <c>NextMessage()</c> or <c>Fetch()</c> manually.
    /// </summary>
    public bool UseBackgroundTask { get; set; } = true;

    public string? StreamName { get; set; }
    /// <summary>
    /// Durable name for JetStream consumer.
    /// - If specified, the server remembers the consumer's state (last acknowledged message).
    /// - If null, an ephemeral consumer is created (state is lost after disconnect).
    /// Recommended for production reliability.
    /// </summary>
    public string? DurableName { get; set; }

    /// <summary>
    /// Queue group name for load balancing (Core NATS only, not JetStream).
    /// - All subscribers in the same queue group share messages (work queue pattern).
    /// - Each message is delivered to only one subscriber in the group.
    /// Useful when scaling multiple worker instances.
    /// </summary>
    public string? QueueGroup { get; set; }

    /// <summary>
    /// Acknowledgement policy (JetStream only).
    /// Defines how messages are acknowledged by the consumer:
    /// - Explicit: Each message must be explicitly acknowledged (default).
    /// - None: No acknowledgements required (fire-and-forget).
    /// - All: Acknowledging a message also acknowledges all prior messages.
    /// </summary>
    public AckPolicy AckPolicy { get; set; } = AckPolicy.Explicit;

    /// <summary>
    /// Delivery policy (JetStream only).
    /// Controls which messages are delivered when the consumer is first created:
    /// - All: Deliver all available messages (default).
    /// - New: Deliver only messages published after the subscription is created.
    /// - Last: Deliver only the most recent message.
    /// - ByStartSequence: Start from a specific sequence number.
    /// - ByStartTime: Start from a specific point in time.
    /// </summary>
    public DeliverPolicy DeliverPolicy { get; set; } = DeliverPolicy.All;

    /// <summary>
    /// Timeout for waiting messages (milliseconds).
    /// Used in JetStream synchronous subscriptions with <c>NextMessage()</c>.
    /// Prevents blocking indefinitely when no messages are available.
    /// Default = 1000 ms.
    /// </summary>
    public int TimeoutMs { get; set; } = 8000;

    /// <summary>
    /// Whether messages should be automatically acknowledged (JetStream).
    /// - If true, messages are acknowledged after successful handling.
    /// - If false, the handler must manually call <c>msg.Ack()</c>, <c>msg.Nak()</c>, or <c>msg.Term()</c>.
    /// Default = true.
    /// </summary>
    public bool AutoAck { get; set; } = true;

    // -----------------------------
    // Extra JetStream options
    // -----------------------------

    /// <summary>
    /// Maximum number of times a message will be redelivered if not acknowledged.
    /// If null, the server default applies (usually infinite).
    /// Useful for retry logic and dead-letter handling.
    /// </summary>
    public int? MaxDeliver { get; set; }

    /// <summary>
    /// Subject filter for JetStream consumers.
    /// If specified, only messages matching this subject (or sub-subjects) are delivered.
    /// Example: "events.orders.*".
    /// </summary>
    public string? FilterSubject { get; set; }

    /// <summary>
    /// Replay policy for JetStream.
    /// Controls how messages are replayed when delivering historical data:
    /// - Instant: Replay as fast as possible (default).
    /// - Original: Replay using the original message timing.
    /// </summary>
    public ReplayPolicy ReplayPolicy { get; set; } = ReplayPolicy.Instant;

    /// <summary>
    /// Maximum number of unacknowledged messages allowed at one time.
    /// If exceeded, the server pauses delivery until some are acknowledged.
    /// Useful for backpressure control.
    /// </summary>
    public int? MaxAckPending { get; set; }

    /// <summary>
    /// Interval (milliseconds) for server idle heartbeats.
    /// If no messages are available, the server sends heartbeat signals to detect stalled consumers.
    /// Helps in monitoring connection health.
    /// </summary>
    public long? IdleHeartbeatMs { get; set; }

    /// <summary>
    /// Enables flow control in JetStream.
    /// When enabled, the server ensures the consumer is not overwhelmed by too many messages.
    /// Recommended for high-throughput consumers.
    /// Default = false.
    /// </summary>
    public bool EnableFlowControl { get; set; } = false;

    /// <summary>
    /// Interval for flow control (JetStream only).
    /// - If specified, sets how frequently the server sends flow control messages to the consumer.
    /// - Only relevant if <see cref="EnableFlowControl"/> is true.
    /// - Default behavior: 30 seconds (can be overridden).
    /// </summary>
    public TimeSpan? FlowControlInterval { get; set; }

    /// <summary>
    /// Dead-letter subject for failed messages.
    /// - If specified, messages that exceed <see cref="MaxDeliver"/> attempts
    ///   are sent to this subject instead of being retried indefinitely.
    /// - Useful for handling poison messages.
    /// </summary>
    public string? DeadLetterSubject { get; set; }


    // ---- DLQ (Dead Letter) ----
    public bool EnableDeadLetter { get; set; } = false;
    public bool AutoCreateDeadLetterStream { get; set; } = false;
    public string? DeadLetterStreamName { get; set; } // e.g. "DLQ_ORDERS"
}
