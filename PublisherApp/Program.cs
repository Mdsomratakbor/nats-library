
// 1️⃣ Setup Configuration
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
var requestReply = serviceProvider.GetRequiredService<IRequestReplyHandler>();


// 7️⃣ Publish messages
//await publisher.PublishAsync("events.test", "Hello NATS!");
//await publisher.PublishAsync("events.queue", "Task 1");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("🛑 Stopping publisher...");
};

var random = new Random();
string[] products = { "Laptop", "Phone", "Tablet", "Headphones", "Camera" };

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        // 1️⃣ Create random object
        var order = new Order
        {
            Id = random.Next(1000, 9999), // random ID
            Product = products[random.Next(products.Length)], // random product
            Quantity = random.Next(1, 10) // random quantity
        };

        // 2️⃣ Serialize to JSON
        var json = JsonSerializer.Serialize(order);

        // 3️⃣ Publish
        await publisher.PublishAsync("events.order-create", json);
        Console.WriteLine($"✅ Published: {json}");

        // 4️⃣ Delay 5s
        await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
    }
}
catch (TaskCanceledException) { }


// 8️⃣ Send request
//var response = await requestReply.RequestAsync<string, string>("events.request", "Hello Server");
//Console.WriteLine($"[Client] Got response: {response}");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();