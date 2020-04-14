using System;
using AspNetCore.SignalR.OrleansIntegration.Tests.BuilderConfigurators;
using Orleans.TestingHost;

namespace AspNetCore.SignalR.OrleansIntegration.Tests
{
    public class TestClusterFixture : IDisposable
    {
        public TestClusterFixture()
        {
            var builder = new TestClusterBuilder(2);
            builder.Options.ServiceId = Guid.NewGuid().ToString();
            builder.AddSiloBuilderConfigurator<TestSiloBuilderConfigurator>();
            builder.AddClientBuilderConfigurator<TestClientBuilderConfigurator>();
            var testCluster = builder.Build();

            testCluster.Deploy();

            TestCluster = testCluster;
        }

        public TestCluster TestCluster { get; }

        public void Dispose()
        {
            TestCluster.Client.Close().Wait();
            TestCluster.StopAllSilos();
        }
    }
}