using System;
using Orleans;
using Orleans.Hosting;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public static class ClientBuilderExtensions
    {
        public static IClientBuilder UseSignalR(this IClientBuilder builder, Action<OrleansSignalRClientOptions> configure = null)
        {
            var clientOptions = new OrleansSignalRClientOptions();
            configure?.Invoke(clientOptions);

            builder.AddSimpleMessageStreamProvider(OrleansSignalRConstants.StreamProviderName,
                    options => { options.FireAndForgetDelivery = clientOptions.FireAndForgetDelivery; })
                .ConfigureApplicationParts(manager => { manager.AddApplicationPart(typeof(IClientGrain).Assembly).WithReferences(); });

            return builder;
        }
    }
}