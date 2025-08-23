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
        opts.ReconnectWait = _settings.Nats.ReconnectWaitMs;
        opts.Timeout = _settings.Nats.TimeoutMs;
        opts.PingInterval = _settings.Nats.PingIntervalMs;
        opts.MaxPingsOut = _settings.Nats.MaxPingsOut;
        opts.NoEcho = _settings.Nats.NoEcho;
        opts.ReconnectBufferSize = _settings.Nats.ReconnectBufferSize;
        // Authentication
        //if (!string.IsNullOrWhiteSpace(_settings.Nats.Username))
        //{
        //    opts.User = _settings.Nats.Username;
        //    opts.Password = _settings.Nats.Password ?? string.Empty;
        //}
        //else if (!string.IsNullOrWhiteSpace(_settings.Nats.Token))
        //{
        //    opts.Token = _settings.Nats.Token;
        //}
        var nc = _settings.Nats;

        // 🔐 Authentication
        switch (nc.AuthMode)
        {
            case AuthMode.UserPassword:
                if (!string.IsNullOrWhiteSpace(nc.Username))
                {
                    opts.User = nc.Username;
                    opts.Password = nc.Password ?? string.Empty;
                }
                break;

            case AuthMode.Token:
                if (!string.IsNullOrWhiteSpace(nc.Token))
                    opts.Token = nc.Token;
                break;

            case AuthMode.CredentialsFile:
                if (!string.IsNullOrWhiteSpace(nc.CredentialsPath) && File.Exists(nc.CredentialsPath))
                    opts.SetUserCredentials(nc.CredentialsPath);
                break;

            //case AuthMode.NKey:
            //    if (!string.IsNullOrWhiteSpace(nc.NKey))
            //    {
            //        // nc.NKey should be the private seed (starts with "SU...")
            //        var keyPair = NKeys.FromSeed(nc.NKey);

            //        opts.SetNkey(keyPair.GetPublicKey(), (sender, args) =>
            //        {
            //            // args.ServerNonce is a byte[] provided by the server
            //            args.SignedNonce = keyPair.Sign(args.ServerNonce);
            //        });
            //    }
            //    break;

            case AuthMode.None:
            default:
                break;
        }

        // 🔒 TLS
        if (nc.TlsEnable)
        {
            opts.Secure = true;

            if (!string.IsNullOrWhiteSpace(nc.TlsCert) &&
                !string.IsNullOrWhiteSpace(nc.TlsKey))
            {
                // Load client cert
                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(nc.TlsCert, nc.TlsKey);
                opts.AddCertificate(cert);
            }

            if (!string.IsNullOrWhiteSpace(nc.TlsCaCert))
            {
                var caCert = new System.Security.Cryptography.X509Certificates.X509Certificate2(nc.TlsCaCert);
                opts.TLSRemoteCertificationValidationCallback = (sender, certificate, chain, errors) =>
                {
                    // Add CA cert validation logic here if needed
                    return true;
                };
            }
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
                    .WithStorageType(_settings.JetStream.Storage.ToString().Equals("File", StringComparison.OrdinalIgnoreCase)
                        ? NATS.Client.JetStream.StorageType.File
                        : NATS.Client.JetStream.StorageType.Memory)
                    .WithRetentionPolicy(_settings.JetStream.Retention.ToString().Equals("Limits", StringComparison.OrdinalIgnoreCase)
                        ? NATS.Client.JetStream.RetentionPolicy.Limits
                        : _settings.JetStream.Retention.ToString().Equals("Interest", StringComparison.OrdinalIgnoreCase)
                            ? NATS.Client.JetStream.RetentionPolicy.Interest
                            : NATS.Client.JetStream.RetentionPolicy.WorkQueue)
                    .WithMaxMessages(_settings.JetStream.MaxMessages)
                    .WithMaxBytes(_settings.JetStream.MaxBytes)
                    .WithMaxConsumers(_settings.JetStream.MaxConsumers)
                    .WithMaxAge(_settings.JetStream.MaxAge.Seconds)
                    .WithReplicas(_settings.JetStream.Replicas)
                   .WithDiscardPolicy(_settings.JetStream.Discard.Equals(NATS.Client.JetStream.DiscardPolicy.New)
                                          ? NATS.Client.JetStream.DiscardPolicy.New
                                          : NATS.Client.JetStream.DiscardPolicy.Old)
                    .WithDuplicateWindow(_settings.JetStream.DuplicatesWindow.Seconds)
                    .WithAllowDirect(_settings.JetStream.AllowDirect)
                    .WithAllowRollup(_settings.JetStream.AllowRollup)
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
