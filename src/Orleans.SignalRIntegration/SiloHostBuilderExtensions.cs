using System;
using Orleans.Hosting;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Grains;

namespace Orleans.SignalRIntegration
{
    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder UseSignalR(this ISiloHostBuilder builder, Action<OrleansSignalRSiloOptions> configure = null)
        {
            var siloOptions = new OrleansSignalRSiloOptions();
            configure?.Invoke(siloOptions);

            builder.AddSimpleMessageStreamProvider(OrleansSignalRConstants.StreamProviderName, options =>
            {
                options.FireAndForgetDelivery = siloOptions.FireAndForgetDelivery;
            });

            if (siloOptions.AddApplicationParts)
                builder.ConfigureApplicationParts(manager =>
                    { manager.AddApplicationPart(typeof(ClientGrain).Assembly).WithReferences(); });

            if (siloOptions.RegisterDefaultSignalRGrainStorage)
                builder.AddMemoryGrainStorage(OrleansSignalRConstants.StorageProviderName);

            if (siloOptions.RegisterDefaultPubSubStorage)
                builder.AddMemoryGrainStorage("PubSubStore");

            return builder;
        }
    }
}