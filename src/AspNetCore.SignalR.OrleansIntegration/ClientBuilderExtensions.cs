using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public static class ClientBuilderExtensions
    {
        public static IClientBuilder UseSignalR(this IClientBuilder builder,
            bool fireAndForgetDelivery = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY)
        {
            builder.AddSimpleMessageStreamProvider(OrleansSignalRConstants.StreamProviderName,
                    options => { options.FireAndForgetDelivery = fireAndForgetDelivery; })
                .ConfigureApplicationParts(manager => { manager.AddApplicationPart(typeof(IClientGrain).Assembly).WithReferences(); });

            return builder;
        }
    }
}