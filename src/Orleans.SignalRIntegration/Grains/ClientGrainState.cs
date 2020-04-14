using System;

namespace Orleans.SignalRIntegration.Grains
{
    public class ClientGrainState
    {
        public bool Connected { get; set; }

        public Guid HubLifetimeManagerId { get; set; }
    }
}