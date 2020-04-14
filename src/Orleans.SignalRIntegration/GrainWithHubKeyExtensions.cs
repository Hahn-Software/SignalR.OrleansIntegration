using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;

namespace Orleans.SignalRIntegration
{
    internal static class GrainWithHubKeyExtensions
    {
        public static string GetHubName(this IGrainWithHubKey grain)
        {
            return grain.GetPrimaryKeyString().Split(Core.GrainWithHubKeyExtensions.GrainWithHubKeySeparator)[0];
        }

        public static string GetId(this IGrainWithHubKey grain)
        {
            return grain.GetPrimaryKeyString().Split(Core.GrainWithHubKeyExtensions.GrainWithHubKeySeparator)[2];
        }
    }
}