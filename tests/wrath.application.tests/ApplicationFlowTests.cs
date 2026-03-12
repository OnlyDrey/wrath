using Wrath.Application;
using Wrath.Domain;

namespace Wrath.Application.Tests;

public class ApplicationFlowTests
{
    [Fact]
    public async Task Create_And_Search_Profile()
    {
        var profiles = new InMemoryProfiles();
        var service = new ConnectionProfileService(profiles);

        await service.CreateConnectionProfileAsync(
            new ConnectionProfileRequest("Prod Web", ProtocolType.Rdp, "prod-web-01", 3389, "admin", "prod", ["prod", "web"]),
            CancellationToken.None);

        var result = await service.SearchConnectionsAsync("web", CancellationToken.None);
        Assert.Single(result);
    }

    [Fact]
    public async Task Launch_RecordsEvents()
    {
        var profiles = new InMemoryProfiles();
        var history = new InMemoryHistory();
        var profile = new ConnectionProfile(Guid.NewGuid(), "SSH", ProtocolType.Ssh, "host", 22, "u", null);
        await profiles.AddAsync(profile, CancellationToken.None);

        var sut = new SessionService(profiles, history, new FakeDispatcher(new LaunchResult(true, "ok")));
        var result = await sut.LaunchSessionAsync(profile.Id, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, history.Items.Count);
        Assert.Contains(history.Items, x => x.EventType == SessionEventType.LaunchRequested);
        Assert.Contains(history.Items, x => x.EventType == SessionEventType.LaunchSucceeded);
    }

    private sealed class InMemoryProfiles : IConnectionProfileRepository
    {
        private readonly List<ConnectionProfile> _items = [];

        public Task AddAsync(ConnectionProfile profile, CancellationToken ct) { _items.Add(profile); return Task.CompletedTask; }
        public Task<ConnectionProfile?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<ConnectionProfile>> SearchAsync(string? query, CancellationToken ct)
        {
            var q = query?.ToLowerInvariant();
            var result = string.IsNullOrWhiteSpace(q)
                ? _items
                : _items.Where(x => x.Name.ToLowerInvariant().Contains(q) || x.Host.ToLowerInvariant().Contains(q)).ToList();
            return Task.FromResult((IReadOnlyList<ConnectionProfile>)result);
        }
        public Task UpdateAsync(ConnectionProfile profile, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class InMemoryHistory : ISessionHistoryRepository
    {
        public List<SessionRecord> Items { get; } = [];
        public Task AddAsync(SessionRecord record, CancellationToken ct) { Items.Add(record); return Task.CompletedTask; }
        public Task<IReadOnlyList<SessionRecord>> GetRecentAsync(int take, CancellationToken ct) => Task.FromResult((IReadOnlyList<SessionRecord>)Items.Take(take).ToList());
    }

    private sealed class FakeDispatcher(LaunchResult result) : IProtocolDispatcher
    {
        public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct) => Task.FromResult(result);
    }
}
