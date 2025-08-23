using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using NATS.Client.JetStream;
using NatsLibrary.Core.Interfaces;
using NatsLibrary.Core.Utils;

namespace NatsLibrary.Core.Services;

public class SubscriberService : INatsSubscriber
{
    private readonly NatsService _natsService;

    public SubscriberService(NatsService natsService)
    {
        _natsService = natsService ?? throw new ArgumentNullException(nameof(natsService));
    }

    public async Task SubscribeAsync<T>(string subject, Func<T, Task> handler, string? queueGroup = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

            if (_natsService.JetStream != null)
            {
                // JetStream subscription
                var subOpts = PushSubscribeOptions.Builder()
                    .WithDurable(queueGroup ?? Guid.NewGuid().ToString())
                    .Build();

                var subscription = _natsService.JetStream.PushSubscribeSync(subject, subOpts);

                // Start background processing
                int timeoutCount = 0;
                const int maxTimeouts = 5; // Set your desired threshold

                await Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Msg? msg = null;
                        try
                        {
                            msg = subscription.NextMessage(); // timeout in ms
                            if (msg == null)
                            {
                                timeoutCount++;
                                Console.WriteLine("Timeout occurred.");
                                if (timeoutCount >= maxTimeouts)
                                {
                                    Console.WriteLine("Max timeouts reached, exiting loop.");
                                    break;
                                }
                                continue;
                            }
                            timeoutCount = 0; // Reset on successful message

                            var data = Encoding.UTF8.GetString(msg.Data);
                            Console.WriteLine($"Received data: {data}");

                            T? obj;
                            try
                            {
                                obj = JsonSerializerHelper.Deserialize<T>(data);
                            }
                            catch (JsonException)
                            {
                                if (typeof(T) == typeof(string))
                                {
                                    obj = (T)(object)data;
                                }
                                else
                                {
                                    obj = default;
                                    Console.WriteLine("Received non-JSON data and cannot convert to type " + typeof(T).Name);
                                }
                            }

                            if (obj != null)
                                await handler(obj);

                            msg.Ack(); // acknowledge JetStream message
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }, cancellationToken);
            }
            else
            {
                // Normal NATS subscription
                IAsyncSubscription? subscription;

                if (!string.IsNullOrWhiteSpace(queueGroup))
                {
                    subscription = _natsService.Connection.SubscribeAsync(subject, queueGroup);
                }
                else
                {
                    subscription = _natsService.Connection.SubscribeAsync(subject);
                }

                subscription.MessageHandler += async (sender, args) =>
                {
                    try
                    {
                        var data = Encoding.UTF8.GetString(args.Message.Data);
                        T? obj;
                        try
                        {
                            obj = JsonSerializerHelper.Deserialize<T>(data);
                        }
                        catch (JsonException)
                        {
                            if (typeof(T) == typeof(string))
                            {
                                obj = (T)(object)data;
                            }
                            else
                            {
                                obj = default;
                                Console.WriteLine("Received non-JSON data and cannot convert to type " + typeof(T).Name);
                            }
                        }

                        if (obj != null)
                            await handler(obj);
                    }
                    catch
                    {
                        // optionally handle or log errors
                    }
                };

                subscription.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error subscribing to subject '{subject}': {ex.Message}");
            throw; // rethrow to allow caller to handle it
        }
    }
}