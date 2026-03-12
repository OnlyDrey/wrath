using Microsoft.Extensions.Logging.Abstractions;
using Wrath.Application;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.Application.Tests;

public class ApplicationFlowTests
{
    [Fact]
    public async Task Create_And_Search_Profile_By_Name_Host_And_Tag()
    {
        var profiles = new InMemoryProfiles();
        var service = new ConnectionProfileService(profiles, NullLogger<ConnectionProfileService>.Instance);

        var create = await service.CreateConnectionProfileAsync(
            new ConnectionProfileRequest("Prod Web", ProtocolType.Rdp, "prod-web-01", 3389, "admin", "prod", ["prod", "web"]),
            CancellationToken.None);

        Assert.True(create.Succeeded);

        var byName = await service.SearchConnectionsAsync("prod", CancellationToken.None);
        var byHost = await service.SearchConnectionsAsync("web-01", CancellationToken.None);
        var byTag = await service.SearchConnectionsAsync("web", CancellationToken.None);

        Assert.Single(byName);
        Assert.Single(byHost);
        Assert.Single(byTag);
    }

    [Fact]
    public async Task Create_Fails_On_Invalid_Profile()
    {
        var profiles = new InMemoryProfiles();
        var service = new ConnectionProfileService(profiles, NullLogger<ConnectionProfileService>.Instance);

        var result = await service.CreateConnectionProfileAsync(
            new ConnectionProfileRequest("a", ProtocolType.Rdp, "host", 3389, null, null, ["prod"]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid connection profile.", result.Message);
    }

    [Fact]
    public async Task Launch_Records_Success_Events_With_Host_And_Status()
    {
        var profiles = new InMemoryProfiles();
        var history = new InMemoryHistory();
        var profile = new ConnectionProfile(Guid.NewGuid(), "SSH", ProtocolType.Ssh, "host", 22, "u", null, [new Tag("ops")]);
        await profiles.AddAsync(profile, CancellationToken.None);

        var sut = new SessionService(profiles, history, new FakeDispatcher(new LaunchResult(true, "ok")), NullLogger<SessionService>.Instance);
        var result = await sut.LaunchSessionAsync(profile.Id, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, history.Items.Count);
        Assert.All(history.Items, e => Assert.Equal("host", e.Host));
        Assert.Contains(history.Items, x => x.EventType == SessionEventType.LaunchSucceeded && x.Succeeded);
    }

    [Fact]
    public async Task Launch_Records_Failure_Error()
    {
        var profiles = new InMemoryProfiles();
        var history = new InMemoryHistory();
        var profile = new ConnectionProfile(Guid.NewGuid(), "RDP", ProtocolType.Rdp, "badhost", 3389, null, null, [new Tag("lab")]);
        await profiles.AddAsync(profile, CancellationToken.None);

        var sut = new SessionService(profiles, history, new FakeDispatcher(new LaunchResult(false, "failed", "process start error")), NullLogger<SessionService>.Instance);
        var result = await sut.LaunchSessionAsync(profile.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        var failed = Assert.Single(history.Items.Where(x => x.EventType == SessionEventType.LaunchFailed));
        Assert.Equal("process start error", failed.ErrorMessage);
    }

    private sealed class InMemoryProfiles : IConnectionProfileRepository
    {
        private readonly List<ConnectionProfile> _items = [];

        public Task AddAsync(ConnectionProfile profile, CancellationToken ct)
        {
            _items.Add(profile);
            return Task.CompletedTask;
        }

        public Task<ConnectionProfile?> GetByIdAsync(Guid id, CancellationToken ct)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<ConnectionProfile>> SearchAsync(string? query, CancellationToken ct)
        {
            var q = query?.ToLowerInvariant();
            var result = string.IsNullOrWhiteSpace(q)
                ? _items
                : _items.Where(x =>
                    x.Name.ToLowerInvariant().Contains(q) ||
                    x.Host.ToLowerInvariant().Contains(q) ||
                    x.Tags.Any(t => t.Value.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Task.FromResult((IReadOnlyList<ConnectionProfile>)result);
        }

        public Task UpdateAsync(ConnectionProfile profile, CancellationToken ct)
        {
            var idx = _items.FindIndex(x => x.Id == profile.Id);
            if (idx >= 0)
            {
                _items[idx] = profile;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryHistory : ISessionHistoryRepository
    {
        public List<SessionRecord> Items { get; } = [];

        public Task AddAsync(SessionRecord record, CancellationToken ct)
        {
            Items.Add(record);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SessionRecord>> GetRecentAsync(int take, CancellationToken ct)
            => Task.FromResult((IReadOnlyList<SessionRecord>)Items.Take(take).ToList());
    }

    private sealed class FakeDispatcher(LaunchResult result) : IProtocolDispatcher
    {
        public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct) => Task.FromResult(result);
    }
}
