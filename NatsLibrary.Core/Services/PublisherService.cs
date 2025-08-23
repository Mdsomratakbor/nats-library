using System;
using System.Text;
using System.Threading.Tasks;
using NatsLibrary.Core.Interfaces;
using NatsLibrary.Core.Utils;
using NATS.Client;
using NATS.Client.JetStream;

namespace NatsLibrary.Core.Services
{
    public class PublisherService : INatsPublisher
    {
        private readonly NatsService _natsService;

        public PublisherService(NatsService natsService)
        {
            _natsService = natsService ?? throw new ArgumentNullException(nameof(natsService));
        }

        /// <summary>
        /// Publish a message to NATS or JetStream
        /// </summary>
        public async Task PublishAsync<T>(string subject, T message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subject))
                    throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

                // Serialize message to JSON
                var payload = JsonSerializerHelper.Serialize(message);
                var data = Encoding.UTF8.GetBytes(payload);

                // Use JetStream if available
                if (_natsService.JetStream != null)
                {
                    // JetStream publish
                    await Task.Run(() => _natsService.JetStream.Publish(subject, data));
                }
                else
                {
                    // Normal NATS publish
                    _natsService.Connection.Publish(subject, data);
                    _natsService.Connection.Flush();
                }
            }
            catch(Exception ex)
            {
               Console.WriteLine($"Error publishing message to subject '{subject}': {ex.Message}");
            }
        }
    }
}
