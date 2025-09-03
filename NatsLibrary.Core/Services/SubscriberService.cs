using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using NATS.Client.JetStream;
using NatsLibrary.Core.Configurations;
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




    //public async Task SubscribeAsync<T>(
    //string subject,
    //Func<T, Task> handler,
    //SubscriberOptions? options = null,
    //CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(subject))
    //        throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

    //    options ??= new SubscriberOptions();

    //    if (_natsService.JetStream != null)
    //    {
    //        // -------------------------
    //        // JetStream consumer config
    //        // -------------------------
    //        var consumerConfig = ConsumerConfiguration.Builder()
    //            .WithDurable(options.DurableName ?? Guid.NewGuid().ToString())
    //            .WithAckPolicy(options.AckPolicy)
    //            .WithDeliverPolicy(options.DeliverPolicy)
    //            .WithReplayPolicy(options.ReplayPolicy);

    //        if (options.MaxDeliver.HasValue)
    //            consumerConfig.WithMaxDeliver(options.MaxDeliver.Value);

    //        if (!string.IsNullOrWhiteSpace(options.FilterSubject))
    //            consumerConfig.WithFilterSubject(options.FilterSubject);

    //        if (options.MaxAckPending.HasValue)
    //            consumerConfig.WithMaxAckPending(options.MaxAckPending.Value);

    //        if (options.IdleHeartbeatMs.HasValue)
    //            consumerConfig.WithIdleHeartbeat(options.IdleHeartbeatMs.Value);

    //        if (options.EnableFlowControl)
    //        {
    //            var interval = options.FlowControlInterval ?? TimeSpan.FromMinutes(2);
    //            consumerConfig.WithFlowControl(100);
    //        }
    //        var subOpts = PushSubscribeOptions.Builder()
    //            .WithConfiguration(consumerConfig.Build())
    //            .Build();

    //        var subscription = _natsService.JetStream.PushSubscribeSync(subject, subOpts);

    //        // -------------------------
    //        // Background processing loop
    //        // -------------------------
    //        await Task.Run(async () =>
    //        {
    //            while (!cancellationToken.IsCancellationRequested)
    //            {
    //                try
    //                {
    //                    var msg = subscription.NextMessage(options.TimeoutMs);
    //                    if (msg == null) continue;

    //                    T? obj;
    //                    var data = Encoding.UTF8.GetString(msg.Data);

    //                    try
    //                    {
    //                        obj = JsonSerializerHelper.Deserialize<T>(data);
    //                    }
    //                    catch (JsonException)
    //                    {
    //                        if (typeof(T) == typeof(string))
    //                            obj = (T)(object)data;
    //                        else
    //                        {
    //                            obj = default;
    //                            Console.WriteLine($"[WARN] Cannot convert message to {typeof(T).Name}: {data}");
    //                        }
    //                    }

    //                    if (obj != null)
    //                    {
    //                        try
    //                        {
    //                            // Execute user handler
    //                            await handler(obj);

    //                            // ✅ ACK only if processing succeeds
    //                            if (options.AutoAck)
    //                                msg.Ack();
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            // ❌ Failed processing, do NOT ack → message will be redelivered
    //                            Console.WriteLine($"[ERROR] Processing failed: {ex.Message}");
    //                            msg.Nak(); // Optional: negative ack to request redelivery immediately
    //                        }
    //                    }
    //                    else
    //                    {
    //                        // Could not deserialize message → Nak to retry
    //                        msg.Nak();
    //                    }
    //                }
    //                catch (NATSJetStreamException ex)
    //                {
    //                    Console.WriteLine($"[JetStream Error] {ex.Message}");
    //                }
    //            }
    //        }, cancellationToken);
    //        // -------------------------
    //        // Dead Letter Queue handling
    //        // -------------------------
    //        if (!string.IsNullOrWhiteSpace(options.DeadLetterSubject))
    //        {
    //            var dlqSubscription = _natsService.JetStream.PushSubscribeSync(
    //                options.DeadLetterSubject,
    //                PushSubscribeOptions.Builder()
    //                    .WithDurable($"{options.DurableName ?? "dlq"}-dlq")
    //                    .Build());

    //            _ = Task.Run(() =>
    //            {
    //                while (!cancellationToken.IsCancellationRequested)
    //                {
    //                    try
    //                    {
    //                        var msg = dlqSubscription.NextMessage(options.TimeoutMs);
    //                        if (msg == null) continue;

    //                        var data = Encoding.UTF8.GetString(msg.Data);
    //                        Console.WriteLine($"[DLQ] Received dead-lettered message: {data}");

    //                        // Here you can handle logging, send alerts, move to another system, etc.
    //                        msg.Ack();
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.WriteLine($"[DLQ Error] {ex.Message}");
    //                    }
    //                }
    //            }, cancellationToken);
    //        }

    //}
    //    else
    //    {
    //        // -------------------------
    //        // Core NATS subscription
    //        // -------------------------
    //        IAsyncSubscription subscription = !string.IsNullOrWhiteSpace(options.QueueGroup)
    //            ? _natsService.Connection.SubscribeAsync(subject, options.QueueGroup)
    //            : _natsService.Connection.SubscribeAsync(subject);

    //        subscription.MessageHandler += async (_, args) =>
    //        {
    //            var data = Encoding.UTF8.GetString(args.Message.Data);
    //            T? obj;

    //            try
    //            {
    //                obj = JsonSerializerHelper.Deserialize<T>(data);
    //            }
    //            catch
    //            {
    //                obj = typeof(T) == typeof(string) ? (T)(object)data : default;
    //            }

    //            if (obj != null)
    //            {
    //                try
    //                {
    //                    await handler(obj);
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine($"[ERROR] Core NATS processing failed: {ex.Message}");
    //                    // Core NATS has no redelivery, you may re-publish to retry queue if needed
    //                }
    //            }
    //        };

    //        subscription.Start();
    //    }
    //}

    public async Task SubscribeAsync<T>(
          string subject,
          Func<T, Task> handler,
          SubscriberOptions? options = null,
          CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

        options ??= new SubscriberOptions();

        switch (options.Pattern)
        {
            case SubscriberPattern.CoreSync:
                await SubscribeCoreSync(subject, handler, options, cancellationToken);
                break;

            case SubscriberPattern.CoreAsync:
                SubscribeCoreAsync(subject, handler, options);
                break;

            case SubscriberPattern.CoreQueue:
                SubscribeCoreQueue(subject, handler, options);
                break;

            case SubscriberPattern.JetStreamPush:
                await SubscribeJsPush(subject, handler, options, cancellationToken);
                break;

            case SubscriberPattern.JetStreamPull:
                await SubscribeJsPull(subject, handler, options, cancellationToken);
                break;

            case SubscriberPattern.JetStreamQueue:
                await SubscribeJsQueue(subject, handler, options, cancellationToken);
                break;

            case SubscriberPattern.JetStreamOrdered:
                await SubscribeJsOrdered(subject, handler, options, cancellationToken);
                break;

            default:
                throw new NotSupportedException($"Pattern {options.Pattern} not supported.");
        }

        // Start DLQ watcher (if configured)
        if (options.EnableDeadLetter)
        {
            StartDeadLetterWatcher(options, cancellationToken);
        }
    }

    // ---------- Core NATS ----------
    private async Task SubscribeCoreSync<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options,
        CancellationToken token)
    {
        var sub = _natsService.Connection.SubscribeSync(subject);

        if (options.UseBackgroundTask)
        {
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Msg msg = null;
                    try
                    {
                        msg = sub.NextMessage(options.TimeoutMs);
                        if (msg == null) continue;
                        await HandleMessage(msg.Data, handler);
                    }
                    catch (NATSConnectionClosedException) { break; }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CoreSync] Error: {ex.Message}");
                    }
                }
            }, token);
        }
        else
        {
            // single tick (manual mode)
            var msg = sub.NextMessage(options.TimeoutMs);
            if (msg != null) await HandleMessage(msg.Data, handler);
        }
    }

    private void SubscribeCoreAsync<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options)
    {
        var sub = _natsService.Connection.SubscribeAsync(subject);
        sub.MessageHandler += async (_, args) =>
        {
            try { await HandleMessage(args.Message.Data, handler); }
            catch (Exception ex) { Console.WriteLine($"[CoreAsync] Handler error: {ex.Message}"); }
        };
        sub.Start();
    }

    private void SubscribeCoreQueue<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.QueueGroup))
            throw new ArgumentException("QueueGroup is required for CoreQueue.", nameof(options.QueueGroup));

        var sub = _natsService.Connection.SubscribeAsync(subject, options.QueueGroup);
        sub.MessageHandler += async (_, args) =>
        {
            try { await HandleMessage(args.Message.Data, handler); }
            catch (Exception ex) { Console.WriteLine($"[CoreQueue] Handler error: {ex.Message}"); }
        };
        sub.Start();
    }

    // ---------- JetStream ----------
    private async Task SubscribeJsPush<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options,
        CancellationToken token)
    {
        var js = _natsService.JetStream ?? _natsService.Connection.CreateJetStreamContext();

        var cc = ConsumerConfiguration.Builder()
            .WithDurable(options.DurableName ?? throw new ArgumentException("DurableName required for JetStream"))
            .WithAckPolicy(options.AckPolicy)
            .WithDeliverPolicy(options.DeliverPolicy)
            .WithReplayPolicy(options.ReplayPolicy);

        if (options.MaxDeliver.HasValue) cc.WithMaxDeliver(options.MaxDeliver.Value);
        if (!string.IsNullOrWhiteSpace(options.FilterSubject)) cc.WithFilterSubject(options.FilterSubject);
        if (options.MaxAckPending.HasValue) cc.WithMaxAckPending(options.MaxAckPending.Value);
        if (options.IdleHeartbeatMs.HasValue) cc.WithIdleHeartbeat(options.IdleHeartbeatMs.Value);
        if (options.EnableFlowControl) cc.WithFlowControl(100);

        var subOpts = PushSubscribeOptions.Builder().WithConfiguration(cc.Build()).Build();

        var sub = string.IsNullOrWhiteSpace(options.QueueGroup)
            ? js.PushSubscribeSync(subject, subOpts)
            : js.PushSubscribeSync(subject, options.QueueGroup, subOpts);

        if (options.UseBackgroundTask)
        {
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var msg = sub.NextMessage(options.TimeoutMs);
                        if (msg == null) continue;
                        await HandleJsMessage(msg, handler, options);
                    }
                    catch (NATSJetStreamException jex)
                    {
                        Console.WriteLine($"[JsPush] JetStream error: {jex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[JsPush] Error: {ex.Message}");
                    }
                }
            }, token);
        }
        else
        {
            var msg = sub.NextMessage(options.TimeoutMs);
            if (msg != null) await HandleJsMessage(msg, handler, options);
        }
    }

    private async Task SubscribeJsPull<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options,
        CancellationToken token)
    {
        var js = _natsService.JetStream ?? _natsService.Connection.CreateJetStreamContext();

        var pullOpts = PullSubscribeOptions.Builder()
            .WithDurable(options.DurableName ?? throw new ArgumentException("DurableName required for pull consumer"))
            .Build();

        var sub = js.PullSubscribe(subject, pullOpts);

        if (options.UseBackgroundTask)
        {
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        foreach (var msg in sub.Fetch(10, options.TimeoutMs))
                        {
                            await HandleJsMessage(msg, handler, options);
                        }
                    }
                    catch (NATSJetStreamException jex)
                    {
                        Console.WriteLine($"[JsPull] JetStream error: {jex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[JsPull] Error: {ex.Message}");
                    }
                }
            }, token);
        }
        else
        {
            foreach (var msg in sub.Fetch(10, options.TimeoutMs))
            {
                await HandleJsMessage(msg, handler, options);
            }
        }
    }

    private async Task SubscribeJsQueue<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options,
        CancellationToken token)
    {
        // same as push, but with queue group required
        if (string.IsNullOrWhiteSpace(options.QueueGroup))
            throw new ArgumentException("QueueGroup is required for JetStreamQueue.", nameof(options.QueueGroup));

        await SubscribeJsPush(subject, handler, options, token);
    }

    private async Task SubscribeJsOrdered<T>(
        string subject,
        Func<T, Task> handler,
        SubscriberOptions options,
        CancellationToken token)
    {
        var js = _natsService.JetStream ?? _natsService.Connection.CreateJetStreamContext();

        var sub = js.PushSubscribeSync(
            subject,
            PushSubscribeOptions.Builder().WithOrdered(true).Build());

        if (options.UseBackgroundTask)
        {
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var msg = sub.NextMessage(options.TimeoutMs);
                        if (msg == null) continue;
                        await HandleJsMessage(msg, handler, options);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[JsOrdered] Error: {ex.Message}");
                    }
                }
            }, token);
        }
        else
        {
            var msg = sub.NextMessage(options.TimeoutMs);
            if (msg != null) await HandleJsMessage(msg, handler, options);
        }
    }

    // ---------- Helpers ----------
    private async Task HandleMessage<T>(byte[] data, Func<T, Task> handler)
    {
        var json = Encoding.UTF8.GetString(data);
        T? obj;
        try
        {
            obj = JsonSerializerHelper.Deserialize<T>(json);
        }
        catch
        {
            obj = typeof(T) == typeof(string) ? (T)(object)json : default;
        }

        if (obj != null)
            await handler(obj);
    }

    private async Task HandleJsMessage<T>(Msg msg, Func<T, Task> handler, SubscriberOptions options)
    {
        try
        {
            await HandleMessage(msg.Data, handler);
            if (options.AutoAck) msg.Ack();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Handler] Error: {ex.Message}");
            // NAK so the message is redelivered up to MaxDeliver
            try { msg.Nak(); } catch { /* ignore */ }
        }
    }

    // ---------- DLQ Watcher ----------
    private void StartDeadLetterWatcher(SubscriberOptions options, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(options.StreamName))
            throw new ArgumentException("StreamName is required when EnableDeadLetter = true.");
        if (string.IsNullOrWhiteSpace(options.DurableName))
            throw new ArgumentException("DurableName is required when EnableDeadLetter = true.");
        if (string.IsNullOrWhiteSpace(options.DeadLetterSubject))
            throw new ArgumentException("DeadLetterSubject is required when EnableDeadLetter = true.");
        if (!options.MaxDeliver.HasValue)
            Console.WriteLine("[DLQ] Warning: MaxDeliver not set. Messages may never reach DLQ advisory.");

        // Optionally ensure a DLQ stream exists so DLQ messages are persisted
        if (options.AutoCreateDeadLetterStream && !string.IsNullOrWhiteSpace(options.DeadLetterStreamName))
        {
            try
            {
                var jsm = _natsService.Connection.CreateJetStreamManagementContext();
                var sc = StreamConfiguration.Builder()
                    .WithName(options.DeadLetterStreamName)
                    .WithSubjects(options.DeadLetterSubject!)
                    .WithStorageType(StorageType.File)
                    .Build();
                jsm.AddStream(sc);
            }
            catch (NATSJetStreamException)
            {
                // likely already exists; ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DLQ] Auto-create stream failed: {ex.Message}");
            }
        }

        // Subscribe to JetStream "max deliveries" advisory for this consumer
        // $JS.EVENT.ADVISORY.CONSUMER.MAX_DELIVERIES.<stream>.<consumer>
        var advisorySubject = $"$JS.EVENT.ADVISORY.CONSUMER.MAX_DELIVERIES.{options.StreamName}.{options.DurableName}";
        var advisorySub = _natsService.Connection.SubscribeAsync(advisorySubject);

        advisorySub.MessageHandler += async (_, args) =>
        {
            try
            {
                var advJson = Encoding.UTF8.GetString(args.Message.Data);
                var adv = JsonSerializer.Deserialize<MaxDeliverAdvisory>(advJson);
                if (adv == null || adv.StreamSeq == 0) return;

                // Fetch original message by stream sequence
                var jsm = _natsService.Connection.CreateJetStreamManagementContext();
                var sm = jsm.GetMessage(options.StreamName!, adv.StreamSeq);

                // Republish to DLQ subject with helpful headers
                var dlqMsg = new Msg(options.DeadLetterSubject!)
                {
                    Data = sm.Data
                };

                var hdr = new MsgHeader();
                //if (sm.Headers != null)
                //{
                //    foreach (var key in sm.Headers)
                //    {
                //        var values = sm.Headers.GetValues(key); // returns IList<string>
                //        if (values != null)
                //        {
                //            foreach (var val in values)
                //            {
                //                hdr.Add(key, val); // both key and val are string
                //            }
                //        }
                //    }
                //}

                hdr["X-DLQ-Stream"] = options.StreamName!;
                hdr["X-DLQ-Consumer"] = options.DurableName!;
                hdr["X-DLQ-Stream-Seq"] = adv.StreamSeq.ToString();
                if (adv.Deliveries > 0) hdr["X-DLQ-Deliveries"] = adv.Deliveries.ToString();
                dlqMsg.Header = hdr;

                _natsService.Connection.Publish(dlqMsg); // publish to DLQ (persisted if a stream captures DeadLetterSubject)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DLQ] Error handling advisory: {ex.Message}");
            }
        };

        advisorySub.Start();

        // Optional: stop watcher on token cancellation
        _ = Task.Run(async () =>
        {
            try
            {
                token.WaitHandle.WaitOne();
                advisorySub?.Unsubscribe();
            }
            catch { /* ignore */ }
        }, token);
    }

    // Advisory payload (fields differ slightly by server; map the common ones)
    private sealed class MaxDeliverAdvisory
    {
        [JsonPropertyName("stream")] public string? Stream { get; set; }
        [JsonPropertyName("consumer")] public string? Consumer { get; set; }

        // stream_seq is the important piece we need to fetch the message
        [JsonPropertyName("stream_seq")] public ulong StreamSeq { get; set; }

        // deliveries / num_delivered depending on version
        [JsonPropertyName("deliveries")] public int Deliveries { get; set; }
        [JsonPropertyName("num_delivered")]
        public int NumDelivered
        {
            get => Deliveries;
            set => Deliveries = value;
        }
    }

}