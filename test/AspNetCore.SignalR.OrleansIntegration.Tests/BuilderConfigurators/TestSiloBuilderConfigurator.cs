using Orleans.Hosting;
using Orleans.SignalRIntegration;
using Orleans.TestingHost;

#pragma warning disable 618

namespace AspNetCore.SignalR.OrleansIntegration.Tests.BuilderConfigurators
{
    public class TestSiloBuilderConfigurator : ISiloBuilderConfigurator
    {
        public void Configure(ISiloHostBuilder hostBuilder)
        {
            hostBuilder.UseSignalR();
        }
    }
}