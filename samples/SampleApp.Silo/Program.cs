using System;
using System.Threading.Tasks;
using System.Timers;
using Orleans;
using Orleans.Hosting;
using Orleans.SignalRIntegration;
using SampleApp.Abstractions;

namespace SampleApp.Silo
{
    internal static class Program
    {
        private static void Main()
        {
            var siloHost = StartSilo().Result;

            var timer = SetupChatServerTimer();

            Console.ReadKey();

            siloHost.StopAsync().Wait();

            siloHost.Dispose();
            timer.Stop();
            timer.Dispose();
        }

        private static Timer SetupChatServerTimer()
        {
            var timer = new Timer {Interval = 10000};

            timer.Elapsed += (sender, e) =>
            {
                var client = new ClientBuilder()
                    .UseLocalhostClustering()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChatGrain).Assembly).WithReferences())
                    .Build();

                client.Connect().Wait();

                var grain = client.GetGrain<IChatGrain>("MyChat");
                grain.SendServerMessage().Wait();
            };

            timer.Start();
            return timer;
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var siloHost = new SiloHostBuilder()
                .UseLocalhostClustering()
                .UseSignalR()
                .AddMemoryGrainStorageAsDefault()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChatGrain).Assembly).WithReferences())
                .Build();

            await siloHost.StartAsync();

            return siloHost;
        }
    }
}