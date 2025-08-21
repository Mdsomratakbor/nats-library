using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Configurations;

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
    public string Storage { get; set; } = "File";

    /// <summary>
    /// Maximum number of messages in the stream (0 = unlimited)
    /// </summary>
    public int MaxMessages { get; set; } = 0;

    /// <summary>
    /// Maximum bytes for the stream (0 = unlimited)
    /// </summary>
    public long MaxBytes { get; set; } = 0;

    /// <summary>
    /// Message retention policy (Limits, Interest, WorkQueue)
    /// </summary>
    public string Retention { get; set; } = "Limits";
}
