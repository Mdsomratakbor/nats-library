using System;

namespace NatsLibrary.Core.Configurations;

/// <summary>
/// Authentication mode for NATS
/// </summary>
public enum AuthMode
{
    None,
    UserPassword,
    Token,
    CredentialsFile,
    NKey
}

/// <summary>
/// Core NATS configuration (connection, authentication, client name)
/// </summary>
public class NatsConfiguration
{
    /// <summary>
    /// NATS server URL (default: nats://localhost:4222)
    /// Can be a comma-separated list for clustered servers
    /// </summary>
    public string Url { get; set; } = "nats://localhost:4222";

    /// <summary>
    /// Authentication mode
    /// </summary>
    public AuthMode AuthMode { get; set; } = AuthMode.None;

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
    /// Path to credentials file (for NATS 2.0 JWT authentication)
    /// </summary>
    public string? CredentialsPath { get; set; }

    /// <summary>
    /// NKey seed or public key (optional)
    /// </summary>
    public string? NKey { get; set; }

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

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Ping interval to check connection health (ms)
    /// </summary>
    public int PingIntervalMs { get; set; } = 120000; // 2 minutes

    /// <summary>
    /// Maximum number of missed pings before disconnect
    /// </summary>
    public int MaxPingsOut { get; set; } = 3;

    /// <summary>
    /// Whether to echo messages back to the publisher (default: true)
    /// </summary>
    public bool NoEcho { get; set; } = false;

    /// <summary>
    /// Buffer size for pending messages while disconnected (0 = unlimited)
    /// </summary>
    public int ReconnectBufferSize { get; set; } = 8 * 1024 * 1024; // 8MB

    /// <summary>
    /// Enable TLS/SSL (default: false)
    /// </summary>
    public bool TlsEnable { get; set; } = false;

    /// <summary>
    /// Path to TLS certificate file
    /// </summary>
    public string? TlsCert { get; set; }

    /// <summary>
    /// Path to TLS private key file
    /// </summary>
    public string? TlsKey { get; set; }

    /// <summary>
    /// Path to TLS CA certificate
    /// </summary>
    public string? TlsCaCert { get; set; }
}
