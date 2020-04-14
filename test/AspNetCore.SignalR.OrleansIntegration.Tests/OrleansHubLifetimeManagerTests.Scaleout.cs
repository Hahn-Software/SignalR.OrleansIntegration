// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using AspNetCore.SignalR.OrleansIntegration.TestUtils.Microsoft;
using Microsoft.AspNetCore.SignalR.Protocol;
using Xunit;

namespace AspNetCore.SignalR.OrleansIntegration.Tests
{
    public partial class OrleansHubLifetimeManagerTests
    {
        private async Task AssertMessageAsync(TestClient client)
        {
            var message = Assert.IsType<InvocationMessage>(await client.ReadAsync().OrTimeout());
            Assert.Equal("Hello", message.Target);
            Assert.Single(message.Arguments);
            Assert.Equal("World", (string) message.Arguments[0]);
        }

        [Fact]
        public async Task AddGroupAsyncForConnectionOnDifferentServerAlreadyInGroupDoesNothing()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout();

                await manager1.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();
                await manager2.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();

                await manager2.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client);
                Assert.Null(client.TryRead());
            }
        }

        [Fact]
        public async Task AddGroupAsyncForConnectionOnDifferentServerWorks()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout();

                await manager2.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();

                await manager2.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client);
            }
        }

        [Fact]
        public async Task InvokeAllAsyncWithMultipleServersDoesNotWriteToDisconnectedConnectionsOutput()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager1.OnConnectedAsync(connection1).OrTimeout();
                await manager2.OnConnectedAsync(connection2).OrTimeout();

                await manager2.OnDisconnectedAsync(connection2).OrTimeout();

                await manager2.SendAllAsync("Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client1);
                Assert.Null(client2.TryRead());
            }
        }

        // ADDED: ForSpecificHub
        [Fact]
        public async Task InvokeAllAsyncWithMultipleServersWritesToAllConnectionsForSpecificHubOutput()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager3 = CreateNewHubLifetimeManager<AnotherHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            using (var client3 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);
                var connection3 = HubConnectionContextUtils.Create(client3.Connection);

                await manager1.OnConnectedAsync(connection1).OrTimeout();
                await manager2.OnConnectedAsync(connection2).OrTimeout();
                await manager3.OnConnectedAsync(connection3).OrTimeout();

                await manager1.SendAllAsync("Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);
                Assert.Null(client3.TryRead());
            }
        }

        [Fact]
        public async Task InvokeAllAsyncWithMultipleServersWritesToAllConnectionsOutput()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager1.OnConnectedAsync(connection1).OrTimeout();
                await manager2.OnConnectedAsync(connection2).OrTimeout();

                await manager1.SendAllAsync("Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);
            }
        }

        [Fact]
        public async Task InvokeConnectionAsyncForLocalConnectionDoesNotPublishToBackplane()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                // Add connection to both "servers" to see if connection receives message twice
                await manager1.OnConnectedAsync(connection).OrTimeout();
                await manager2.OnConnectedAsync(connection).OrTimeout();

                await manager1.SendConnectionAsync(connection.ConnectionId, "Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client);
                Assert.Null(client.TryRead());
            }
        }

        [Fact]
        public async Task InvokeConnectionAsyncOnServerWithoutConnectionWritesOutputToConnection()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout();

                await manager2.SendConnectionAsync(connection.ConnectionId, "Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client);
            }
        }

        [Fact]
        public async Task InvokeGroupAsyncOnServerWithoutConnectionWritesOutputToGroupConnection()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout();

                await manager1.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();

                await manager2.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client);
            }
        }

        [Fact]
        public async Task RemoveGroupAsyncForConnectionOnDifferentServerWorks()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout();

                await manager1.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();

                await manager2.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client);

                await manager2.RemoveFromGroupAsync(connection.ConnectionId, "group").OrTimeout();

                await manager2.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();

                Assert.Null(client.TryRead());
            }
        }

        [Fact]
        public async Task RemoveGroupFromConnectionOnDifferentServerNotInGroupDoesNothing()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout();

                await manager2.RemoveFromGroupAsync(connection.ConnectionId, "group").OrTimeout();
            }
        }

        [Fact]
        public async Task WritingToRemoteConnectionThatFailsDoesNotThrow()
        {
            using (var manager1 = CreateNewHubLifetimeManager<MyHub>())
            using (var manager2 = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                // Force an exception when writing to connection
                var connectionMock = HubConnectionContextUtils.CreateMock(client.Connection);

                await manager2.OnConnectedAsync(connectionMock).OrTimeout();

                // This doesn't throw because there is no connection.ConnectionId on this server so it has to publish to the backplane.
                // And once that happens there is no way to know if the invocation was successful or not.
                await manager1.SendConnectionAsync(connectionMock.ConnectionId, "Hello", new object[] {"World"}).OrTimeout();
            }
        }
    }
}