using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NATS.Client;
using NATS.Client.JetStream;

namespace NatsLibrary.Core.Services
{
    /// <summary>
    /// Central NATS service containing connection + JetStream context
    /// </summary>
    public class NatsService
    {
        public IConnection Connection { get; }
        public IJetStream? JetStream { get; }

        public NatsService(IConnection connection, IJetStream? jetStream = null)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            JetStream = jetStream;
        }
    }
}
