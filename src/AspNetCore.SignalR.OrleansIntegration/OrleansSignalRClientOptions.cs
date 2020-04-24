using Orleans.Configuration;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public class OrleansSignalRClientOptions
    {
        public bool FireAndForgetDelivery { get; set; } = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY;
    }
}