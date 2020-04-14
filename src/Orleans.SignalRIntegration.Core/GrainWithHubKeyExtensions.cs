using System;

namespace Orleans.SignalRIntegration.Core
{
    public static class GrainWithHubKeyExtensions
    {
        public const char GrainWithHubKeySeparator = ':';

        public static string ToGrainHubKey(this string id, string hubName, string scope = null)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (scope == null)
                return $"{hubName}{GrainWithHubKeySeparator}{id}";

            return $"{hubName}{GrainWithHubKeySeparator}{scope}{GrainWithHubKeySeparator}{id}";
        }
    }
}