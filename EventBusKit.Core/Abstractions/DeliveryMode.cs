using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusKit.Core.Abstractions
{

    /// <summary>
    /// Defines message delivery modes.
    /// </summary>
    public enum DeliveryMode
    {
        /// <summary>
        /// Message is delivered at most once.
        /// </summary>
        AtMostOnce,

        /// <summary>
        /// Message is delivered at least once.
        /// </summary>
        AtLeastOnce,

        /// <summary>
        /// Message is delivered exactly once (if supported by broker).
        /// </summary>
        ExactlyOnce
    }
}
