using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Configurations;

/// <summary>
/// Core NATS configuration (connection, authentication, client name)
/// </summary>
public class NatsConfiguration
{
    /// <summary>
    /// NATS server URL (default: nats://localhost:4222)
    /// </summary>
    public string Url { get; set; } = "nats://localhost:4222";

    /// <summary>
    /// Username for authentication (optional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for authentication (optional)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Token for authentication (optional)
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Client name (default: "NatsClient")
    /// </summary>
    public string ClientName { get; set; } = "NatsClient";

    /// <summary>
    /// Reconnect attempts (-1 = infinite)
    /// </summary>
    public int MaxReconnect { get; set; } = -1;

    /// <summary>
    /// Reconnect wait in milliseconds
    /// </summary>
    public int ReconnectWaitMs { get; set; } = 2000;
}
