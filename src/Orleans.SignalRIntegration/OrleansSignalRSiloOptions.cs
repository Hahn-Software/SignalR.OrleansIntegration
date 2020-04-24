using Orleans.Configuration;

namespace Orleans.SignalRIntegration
{
    public class OrleansSignalRSiloOptions
    {
        public bool FireAndForgetDelivery { get; set; } = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY;

        public bool RegisterDefaultPubSubStorage { get; set; } = true;
        
        public bool RegisterDefaultSignalRGrainStorage { get; set; } = true;
    }
}