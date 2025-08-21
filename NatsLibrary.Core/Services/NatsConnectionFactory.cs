using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Threading.Tasks;
using NatsLibrary.Core.Configurations;

namespace NatsLibrary.Core.Services;

public class NatsConnectionFactory
{
    private readonly NatsLibrarySettings _settings;

    public NatsConnectionFactory(NatsLibrarySettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Create a NATS connection (and JetStream context if enabled)
    /// </summary>
    public async Task<NatsService> CreateAsync()
    {
        // Build connection options
        var opts = ConnectionFactory.GetDefaultOptions();
        opts.Url = _settings.Nats.Url;
        opts.Name = _settings.Nats.ClientName;
        opts.MaxReconnect = _settings.Nats.MaxReconnect;
        opts.ReconnectWait = TimeSpan.FromMilliseconds(_settings.Nats.ReconnectWaitMs).Seconds;

        // Authentication
        if (!string.IsNullOrWhiteSpace(_settings.Nats.Username))
        {
            opts.User = _settings.Nats.Username;
            opts.Password = _settings.Nats.Password ?? string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(_settings.Nats.Token))
        {
            opts.Token = _settings.Nats.Token;
        }

        // Create connection
        var factory = new ConnectionFactory();
        IConnection conn = factory.CreateConnection(opts);

        // Create JetStream context if enabled
        IJetStream? js = null;
        if (_settings.JetStream.Enable)
        {
            js = conn.CreateJetStreamContext();

            // Create management context for streams
            var jsm = conn.CreateJetStreamManagementContext();

            if (!string.IsNullOrWhiteSpace(_settings.JetStream.StreamName)
                && _settings.JetStream.Subjects.Length > 0)
            {
                var streamConfig = StreamConfiguration.Builder()
                    .WithName(_settings.JetStream.StreamName)
                    .WithSubjects(_settings.JetStream.Subjects)
                    .WithStorageType(_settings.JetStream.Storage.Equals("File", StringComparison.OrdinalIgnoreCase)
                        ? StorageType.File
                        : StorageType.Memory)
                    .WithRetentionPolicy(_settings.JetStream.Retention.Equals("Limits", StringComparison.OrdinalIgnoreCase)
                        ? RetentionPolicy.Limits
                        : _settings.JetStream.Retention.Equals("Interest", StringComparison.OrdinalIgnoreCase)
                            ? RetentionPolicy.Interest
                            : RetentionPolicy.WorkQueue)
                    .WithMaxMessages(_settings.JetStream.MaxMessages)
                    .WithMaxBytes(_settings.JetStream.MaxBytes)
                    .Build();

                try
                {
                    jsm.AddStream(streamConfig); // <-- Fixed: use management context
                }
                catch (NATSJetStreamException ex) when (ex.ErrorCode == 10073)
                {
                    // Stream already exists, ignore
                }
            }
        }

        return new NatsService(conn, js);
    }
}
