using Wrath.Domain;

namespace Wrath.Application;

public sealed class SessionService
{
    private readonly IConnectionProfileRepository _profiles;
    private readonly ISessionHistoryRepository _history;
    private readonly IProtocolDispatcher _dispatcher;

    public SessionService(
        IConnectionProfileRepository profiles,
        ISessionHistoryRepository history,
        IProtocolDispatcher dispatcher)
    {
        _profiles = profiles;
        _history = history;
        _dispatcher = dispatcher;
    }

    public async Task<LaunchResult> LaunchSessionAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _profiles.GetByIdAsync(profileId, ct) ?? throw new InvalidOperationException("Profile not found.");
        await RecordSessionEventAsync(new SessionRecord(profileId, profile.Protocol, SessionEventType.LaunchRequested, "Launch requested."), ct);

        var result = await _dispatcher.LaunchAsync(profile, ct);
        var evt = result.Succeeded ? SessionEventType.LaunchSucceeded : SessionEventType.LaunchFailed;
        await RecordSessionEventAsync(new SessionRecord(profileId, profile.Protocol, evt, result.Message), ct);
        return result;
    }

    public Task RecordSessionEventAsync(SessionRecord record, CancellationToken ct) => _history.AddAsync(record, ct);

    public Task<IReadOnlyList<SessionRecord>> GetRecentAsync(int take, CancellationToken ct) => _history.GetRecentAsync(take, ct);
}
