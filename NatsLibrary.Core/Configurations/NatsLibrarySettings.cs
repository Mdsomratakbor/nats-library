using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatsLibrary.Core.Configurations;

/// <summary>
/// Full NATS library settings (Core + JetStream)
/// </summary>
public class NatsLibrarySettings
{
    public NatsConfiguration Nats { get; set; } = new NatsConfiguration();
    public JetStreamConfiguration JetStream { get; set; } = new JetStreamConfiguration();
}
