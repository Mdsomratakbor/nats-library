using System;

namespace NatsLibrary.Core.Configurations;

/// <summary>
/// Storage types for JetStream
/// </summary>
public enum StorageType
{
    Memory,
    File
}

/// <summary>
/// Retention policies for JetStream
/// </summary>
public enum RetentionPolicy
{
    Limits,
    Interest,
    WorkQueue
}

/// <summary>
/// Discard policy when stream reaches limits
/// </summary>
public enum DiscardPolicy
{
    Old, // Discard oldest messages
    New  // Reject new messages
}

/// <summary>
/// JetStream specific configuration
/// </summary>
public class JetStreamConfiguration
{
    /// <summary>
    /// Enable JetStream (default: true)
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Default stream name
    /// </summary>
    public string StreamName { get; set; } = "default-stream";

    /// <summary>
    /// Subjects to bind to the stream
    /// </summary>
    public string[] Subjects { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Storage type (Memory or File)
    /// </summary>
    public StorageType Storage { get; set; } = StorageType.File;

    /// <summary>
    /// Message retention policy (Limits, Interest, WorkQueue)
    /// </summary>
    public RetentionPolicy Retention { get; set; } = RetentionPolicy.Limits;

    /// <summary>
    /// Maximum number of messages in the stream (0 = unlimited)
    /// </summary>
    public long MaxMessages { get; set; } = 0;

    /// <summary>
    /// Maximum total bytes for the stream (0 = unlimited)
    /// </summary>
    public long MaxBytes { get; set; } = 0;

    /// <summary>
    /// Maximum message age before expiration (TimeSpan.Zero = unlimited)
    /// </summary>
    public TimeSpan MaxAge { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Maximum number of consumers allowed (-1 = unlimited)
    /// </summary>
    public int MaxConsumers { get; set; } = -1;

    /// <summary>
    /// Discard policy when stream reaches limits
    /// </summary>
    public DiscardPolicy Discard { get; set; } = DiscardPolicy.Old;

    /// <summary>
    /// Number of replicas for HA (default: 1)
    /// </summary>
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Duplicate window time (default: 2 minutes)
    /// Controls duplicate message detection
    /// </summary>
    public TimeSpan DuplicatesWindow { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Allow direct access without consumers (default: false)
    /// </summary>
    public bool AllowDirect { get; set; } = false;

    /// <summary>
    /// Allow roll-up headers for subjects (default: false)
    /// </summary>
    public bool AllowRollup { get; set; } = false;
}
