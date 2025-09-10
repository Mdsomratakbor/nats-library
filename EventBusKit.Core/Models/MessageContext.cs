using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusKit.Core.Models
{
    public class MessageContext
    {
        /// <summary>
        /// Unique identifier for this message.
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Used for request/reply correlation.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Arbitrary metadata headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }
            = new Dictionary<string, string>();

        /// <summary>
        /// When the message was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
