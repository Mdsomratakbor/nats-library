using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusKit.Core.Models
{
    /// <summary>
    /// CloudEvents v1.0 compliant event model.
    /// Reference: https://cloudevents.io/
    /// </summary>
    public class CloudEvent<T>
    {
        /// <summary>
        /// Identifies the event. Producers MUST ensure IDs are unique.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The source that emitted the event (e.g. service name, URI).
        /// </summary>
        public string Source { get; set; } = "urn:default-source";

        /// <summary>
        /// Type of the event (e.g. com.aquilax.order.created).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Version of the CloudEvents specification.
        /// </summary>
        public string SpecVersion { get; set; } = "1.0";

        /// <summary>
        /// Timestamp when the event occurred.
        /// </summary>
        public DateTime Time { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Schema describing the data payload (optional).
        /// </summary>
        public string? DataSchema { get; set; }

        /// <summary>
        /// Content type of the data (e.g. application/json).
        /// </summary>
        public string? DataContentType { get; set; } = "application/json";

        /// <summary>
        /// Subject of the event in the context of the producer (optional).
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// The actual payload of the event.
        /// </summary>
        public T Data { get; set; }
    }
}
