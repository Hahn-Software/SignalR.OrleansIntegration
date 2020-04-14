using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Grains;

namespace Orleans.SignalRIntegration
{
    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder UseSignalR(this ISiloHostBuilder builder, bool registerStorageProviders = true,
            bool fireAndForgetDelivery = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY)
        {
            builder.AddSimpleMessageStreamProvider(OrleansSignalRConstants.StreamProviderName,
                    options => { options.FireAndForgetDelivery = fireAndForgetDelivery; })
                .ConfigureApplicationParts(manager => { manager.AddApplicationPart(typeof(ClientGrain).Assembly).WithReferences(); })
                .AddMemoryGrainStorage(OrleansSignalRConstants.StorageProviderName);

            if (registerStorageProviders)
                builder.AddMemoryGrainStorage("PubSubStore");

            return builder;
        }
    }
}