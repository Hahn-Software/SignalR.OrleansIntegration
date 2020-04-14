using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;

namespace Orleans.SignalRIntegration.Grains
{
    [StorageProvider(ProviderName = OrleansSignalRConstants.StorageProviderName)]
    public class UserGrain : ClientsGrain<UserGrainState>, IUserGrain
    {
        public UserGrain(ILogger<UserGrain> logger) : base(logger)
        {
        }
    }
}