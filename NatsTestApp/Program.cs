// 1️⃣ Setup Configuration
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.JetStream;
using NatsLibrary.Core.Configurations;
using NatsLibrary.Core.Extensions;
using NatsLibrary.Core.Interfaces;
using PublisherApp;

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
_ = subscriber.SubscribeAsync<string>("events.test", async message =>
{
    Console.WriteLine($"[Subscriber] Received: {message}");
}, (string?)null);




// 5️⃣ Queue Group Example
_ = queueService.SubscribeQueueAsync<string>("events.queue", "workers", async message =>
{
    Console.WriteLine($"[Queue Worker] Processing: {message}");
    await Task.Delay(500); // simulate work
});

// 6️⃣ Request / Reply Example
_ = requestReply.ReplyAsync<string, string>("events.request", async request =>
{
    Console.WriteLine($"[Request Handler] Got request: {request}");
    return $"Reply: {request.ToUpper()}";
});

// 7️⃣ Publish messages
await publisher.PublishAsync("events.test", "Hello NATS!");
await publisher.PublishAsync("events.queue", "Task 1");

// 8️⃣ Send request
var response = await requestReply.RequestAsync<string, string>("events.request", "Hello Server");
Console.WriteLine($"[Client] Got response: {response}");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();