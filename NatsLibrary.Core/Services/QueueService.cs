using System;
using System.Text;
using System.Threading.Tasks;
using NATS.Client;
using NATS.Client.JetStream;
using NatsLibrary.Core.Interfaces;
using NatsLibrary.Core.Utils;

namespace NatsLibrary.Core.Services
{
    public class QueueService : IQueueSubscriber
    {
        private readonly NatsService _natsService;

        public QueueService(NatsService natsService)
        {
            _natsService = natsService ?? throw new ArgumentNullException(nameof(natsService));
        }

        public async Task SubscribeQueueAsync<T>(string subject, string queueGroup, Func<T, Task> handler)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

            if (string.IsNullOrWhiteSpace(queueGroup))
                throw new ArgumentException("Queue group cannot be null or empty", nameof(queueGroup));

            // ✅ JetStream subscription using PullSubscribeOptions  
            if (_natsService.JetStream != null)
            {
                // Build PullSubscribeOptions for durable consumer (queue group emulation)  
                var opts = PullSubscribeOptions.Builder()
                    .WithDurable(queueGroup)
                    .Build();

                // Create subscription  
                var subscription = _natsService.JetStream.PullSubscribe(subject, opts);

                // Start message loop  
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            subscription.Pull(10); // Pull a batch of messages  
                            var messages = subscription.Fetch(10, 1000); // Fetch messages with a max wait time of 1000ms  

                            foreach (var msg in messages)
                            {
                                try
                                {
                                    var data = Encoding.UTF8.GetString(msg.Data);
                                    var obj = JsonSerializerHelper.Deserialize<T>(data);
                                    if (obj != null)
                                        await handler(obj);

                                    msg.Ack();
                                }
                                catch
                                {
                                    // optionally log error  
                                }
                            }
                        }
                        catch
                        {
                            // optionally log error  
                        }
                    }
                });
            }
            else
            {
                // Normal NATS queue subscription  
                var subscription = _natsService.Connection.SubscribeAsync(subject, queueGroup);
                subscription.MessageHandler += async (sender, args) =>
                {
                    try
                    {
                        var data = Encoding.UTF8.GetString(args.Message.Data);
                        var obj = JsonSerializerHelper.Deserialize<T>(data);
                        if (obj != null)
                            await handler(obj);
                    }
                    catch
                    {
                        // optionally log error  
                    }
                };
                subscription.Start();
            }
        }
    }
}
