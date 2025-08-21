using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace NatsLibrary.Core.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Start NATS background services (if needed)
        /// </summary>
        public static IApplicationBuilder UseNatsLibrary(this IApplicationBuilder app)
        {
            // Example: can resolve subscribers or background tasks here if needed
            var serviceProvider = app.ApplicationServices;

            // You could start background subscribers here
            // var subscriber = serviceProvider.GetRequiredService<INatsSubscriber>();
            // subscriber.SubscribeAsync<YourType>("subject", async msg => { ... });

            return app;
        }
    }
}
