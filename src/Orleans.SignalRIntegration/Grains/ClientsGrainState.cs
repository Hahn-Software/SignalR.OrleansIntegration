using System;
using System.Collections.Generic;
using Orleans.Streams;

namespace Orleans.SignalRIntegration.Grains
{
    public abstract class ClientsGrainState
    {
        public Dictionary<string, StreamSubscriptionHandle<EventArgs>> HandlesByConnectionId { get; set; }
            = new Dictionary<string, StreamSubscriptionHandle<EventArgs>>();
    }
}