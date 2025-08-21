using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NatsLibrary.Core.Configurations;
using NatsLibrary.Core.Interfaces;
using NatsLibrary.Core.Services;

namespace NatsLibrary.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register NATS library services with DI
        /// </summary>
        public static IServiceCollection AddNatsLibrary(this IServiceCollection services, Action<NatsLibrarySettings>? configure = null, IConfiguration? configuration = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            NatsLibrarySettings settings;

            if (configure != null)
            {
                // Configure programmatically
                settings = new NatsLibrarySettings();
                configure(settings);
            }
            else if (configuration != null)
            {
                // Bind from IConfiguration
                settings = new NatsLibrarySettings();
                configuration.GetSection("NatsLibrary").Bind(settings);
            }
            else
            {
                throw new ArgumentException("Either configure action or IConfiguration must be provided.");
            }

            // Register settings as singleton
            services.AddSingleton(settings);

            // Create NatsService and register
            var factory = new NatsConnectionFactory(settings);
            var natsService = factory.CreateAsync().GetAwaiter().GetResult(); // sync block during startup
            services.AddSingleton(natsService);

            // Register services
            services.AddSingleton<INatsPublisher, PublisherService>();
            services.AddSingleton<INatsSubscriber, SubscriberService>();
            services.AddSingleton<IRequestReplyHandler, RequestReplyService>();
            services.AddSingleton<IQueueSubscriber, QueueService>();

            return services;
        }
    }
}
