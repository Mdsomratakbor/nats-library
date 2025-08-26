// S// 1️⃣ Setup Configuration
using ConsumerApp;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NatsLibrary.Core.Extensions;
using NatsLibrary.Core.Interfaces;
using NATS.Client.JetStream;
using NatsLibrary.Core.Configurations;


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// 2️⃣ Setup DI
var services = new ServiceCollection();
services.AddNatsLibrary(configuration: configuration);

var serviceProvider = services.BuildServiceProvider();

// 3️⃣ Resolve services
var publisher = serviceProvider.GetRequiredService<INatsPublisher>();
var subscriber = serviceProvider.GetRequiredService<INatsSubscriber>();
var queueService = serviceProvider.GetRequiredService<IQueueSubscriber>();
var requestReply = serviceProvider.GetRequiredService<IRequestReplyHandler>();

// 4️⃣ Normal Publish / Subscribe
//_ = subscriber.SubscribeAsync<string>("events.test", async message =>
//{
//    Console.WriteLine($"[Subscriber] Received: {message}");
//});

//_ = subscriber.SubscribeAsync<string>("events.order-create", async message =>
//{
//    var order = JsonSerializer.Deserialize<Order>(message);
//    Console.WriteLine($"📦 Received Order -> Id: {order.Id}, Product: {order.Product}, Quantity: {order.Quantity}");
//    await Task.Delay(TimeSpan.FromMinutes(10));

//    await Task.CompletedTask;
//}, (string)null);


var options = new SubscriberOptions
{
    DurableName = "order-worker",
    QueueGroup = "order-service",
    AckPolicy = AckPolicy.Explicit,
    DeliverPolicy = DeliverPolicy.New,
    TimeoutMs = 9000,
    AutoAck = true,
    MaxDeliver = 5,
    FilterSubject = null,
    ReplayPolicy = ReplayPolicy.Instant,
    MaxAckPending = 100,
    IdleHeartbeatMs = 10_000,
    EnableFlowControl = true,
    FlowControlInterval = TimeSpan.FromSeconds(30)
};

await subscriber.SubscribeAsync<string>(
    "events.order-create",
    async message =>
    {
        var order = JsonSerializer.Deserialize<Order>(message);
        if (order == null)
            throw new Exception("Invalid order JSON!");

        Console.WriteLine($"📦 Processing Order -> Id: {order.Id}, Product: {order.Product}");

        // simulate possible processing failure
        if (order.Quantity > 5)
            throw new Exception("Simulated processing error");

        await Task.Delay(2000); // simulate work
    },
    options
);


//// 5️⃣ Queue Group Example
//_ = queueService.SubscribeQueueAsync<string>("events.queue", "workers", async message =>
//{
//    Console.WriteLine($"[Queue Worker] Processing: {message}");
//    await Task.Delay(500); // simulate work
//});

//// 6️⃣ Request / Reply Example
//_ = requestReply.ReplyAsync<string, string>("events.request", async request =>
//{
//    Console.WriteLine($"[Request Handler] Got request: {request}");
//    return $"Reply: {request.ToUpper()}";
//});

// 7️⃣ Publish messages
//await publisher.PublishAsync("events.test", "Hello NATS!");
//await publisher.PublishAsync("events.queue", "Task 1");

// 8️⃣ Send request
//var response = await requestReply.RequestAsync<string, string>("events.request", "Hello Server");
//Console.WriteLine($"[Client] Got response: {response}");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();