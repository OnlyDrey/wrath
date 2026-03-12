using Microsoft.Extensions.Logging;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.Application;

public sealed class SessionService
{
    private readonly IConnectionProfileRepository _profiles;
    private readonly ISessionHistoryRepository _history;
    private readonly IProtocolDispatcher _dispatcher;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        IConnectionProfileRepository profiles,
        ISessionHistoryRepository history,
        IProtocolDispatcher dispatcher,
        ILogger<SessionService> logger)
    {
        _profiles = profiles;
        _history = history;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<OperationResult> LaunchSessionAsync(Guid profileId, CancellationToken ct)
    {
        try
        {
            var profile = await _profiles.GetByIdAsync(profileId, ct);
            if (profile is null)
            {
                return OperationResult.Failure("Connection profile not found.");
            }

            await RecordSessionEventAsync(new SessionRecord(
                profileId,
                profile.Protocol,
                profile.Host,
                SessionEventType.LaunchRequested,
                succeeded: true,
                "Launch requested."), ct);

            _logger.LogInformation("Protocol launch attempt. ProfileId={ProfileId} Protocol={Protocol} Host={Host}",
                profile.Id, profile.Protocol, profile.Host);

            var result = await _dispatcher.LaunchAsync(profile, ct);
            var eventType = result.Succeeded ? SessionEventType.LaunchSucceeded : SessionEventType.LaunchFailed;

            await RecordSessionEventAsync(new SessionRecord(
                profileId,
                profile.Protocol,
                profile.Host,
                eventType,
                result.Succeeded,
                result.Message,
                result.Error), ct);

            if (result.Succeeded)
            {
                _logger.LogInformation("Protocol launch success. ProfileId={ProfileId} Protocol={Protocol} Host={Host}",
                    profile.Id, profile.Protocol, profile.Host);
                return OperationResult.Success(result.Message);
            }

            _logger.LogWarning("Protocol launch failed. ProfileId={ProfileId} Protocol={Protocol} Host={Host} Error={Error}",
                profile.Id, profile.Protocol, profile.Host, result.Error ?? result.Message);
            return OperationResult.Failure("Protocol launch failed.", result.Error ?? result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected session launch failure. ProfileId={ProfileId}", profileId);
            return OperationResult.Failure("Unexpected launch error.", ex.Message);
        }
    }

    public Task RecordSessionEventAsync(SessionRecord record, CancellationToken ct) => _history.AddAsync(record, ct);

    public Task<IReadOnlyList<SessionRecord>> GetRecentAsync(int take, CancellationToken ct) => _history.GetRecentAsync(take, ct);
}
