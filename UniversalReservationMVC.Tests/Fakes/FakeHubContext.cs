using Microsoft.AspNetCore.SignalR;

namespace UniversalReservationMVC.Tests.Fakes
{
    public class FakeHubContext<THub> : IHubContext<THub> where THub : Hub
    {
        public IHubClients Clients { get; }
        public IGroupManager Groups { get; }

        public FakeClientProxy GroupProxy { get; } = new();

        public FakeHubContext()
        {
            Clients = new FakeHubClients(GroupProxy);
            Groups = new FakeGroupManager();
        }

        private sealed class FakeHubClients : IHubClients
        {
            private readonly FakeClientProxy _groupProxy;

            public FakeHubClients(FakeClientProxy groupProxy)
            {
                _groupProxy = groupProxy;
            }

            public IClientProxy All => new FakeClientProxy();
            public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
            public IClientProxy Client(string connectionId) => new FakeClientProxy();
            public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeClientProxy();
            public IClientProxy Group(string groupName) => _groupProxy;
            public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => _groupProxy;
            public IClientProxy Groups(IReadOnlyList<string> groupNames) => _groupProxy;
            public IClientProxy User(string userId) => new FakeClientProxy();
            public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeClientProxy();
        }

        public sealed class FakeClientProxy : IClientProxy
        {
            public List<(string method, object?[] args)> Sent { get; } = new();

            public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
            {
                Sent.Add((method, args));
                return Task.CompletedTask;
            }
        }

        private sealed class FakeGroupManager : IGroupManager
        {
            public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}