using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace AspNetCore.SignalR.OrleansIntegration
{
    internal static class AsyncStreamExtensions
    {
        public static async Task<StreamSubscriptionHandle<T>> SubscribeWithRetryAsync<T>(this IAsyncStream<T> stream, int retries, int retryInterval,
            Func<T, StreamSequenceToken, Task> onNextAsync)
        {
            var currentTry = 0;

            while (true)
            {
                try
                {
                    return await stream.SubscribeAsync(onNextAsync);
                }
                catch (TimeoutException)
                {
                    if (currentTry >= retries)
                        throw;
                }

                currentTry++;

                await Task.Delay(retryInterval);
            }
        }
    }
}