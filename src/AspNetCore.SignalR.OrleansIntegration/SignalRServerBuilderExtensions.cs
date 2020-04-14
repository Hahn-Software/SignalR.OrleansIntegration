using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public static class SignalRServerBuilderExtensions
    {
        /// <summary>
        ///     Adds scale-out to a <see cref="ISignalRServerBuilder" />, using a shared Orleans cluster.
        /// </summary>
        /// <param name="signalrBuilder">The <see cref="ISignalRServerBuilder" /> to which Orleans integration should be added.</param>
        /// <param name="configure">Action to configure the SignalR Orleans Integration.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder" /> for chaining.</returns>
        public static ISignalRServerBuilder AddOrleans<THub>(this ISignalRServerBuilder signalrBuilder,
            Action<OrleansSignalROptions<THub>> configure = null)
            where THub : Hub
        {
            if (configure != null)
                signalrBuilder.Services.Configure(configure);

            signalrBuilder.Services.AddSingleton(typeof(HubLifetimeManager<THub>), c =>
            {
                var options = c.GetRequiredService<IOptions<OrleansSignalROptions<THub>>>();
                var clusterClient = options.Value.ClusterClient ?? c.GetRequiredService<IClusterClient>();
                var hubProxy = c.GetRequiredService<IHubProxy<THub>>();
                var logger = c.GetRequiredService<ILogger<OrleansHubLifetimeManager<THub>>>();

                return new OrleansHubLifetimeManager<THub>(options, clusterClient, hubProxy, logger);
            });

            signalrBuilder.Services.AddSingleton(typeof(IHubProxy<THub>), c =>
            {
                var options = c.GetRequiredService<IOptions<OrleansSignalROptions<THub>>>().Value;
                var clusterClient = options.ClusterClient ?? c.GetRequiredService<IClusterClient>();

                return new HubProxy<THub>(clusterClient);
            });

            return signalrBuilder;
        }
    }
}