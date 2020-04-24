using Orleans.Providers;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;

namespace Orleans.SignalRIntegration.Grains
{
    [StorageProvider(ProviderName = OrleansSignalRConstants.StorageProviderName)]
    public class GroupGrain : ClientsGrain<GroupGrainState>, IGroupGrain
    {
    }
}