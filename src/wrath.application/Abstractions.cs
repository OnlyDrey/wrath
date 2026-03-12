using Wrath.Domain;

namespace Wrath.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IProtocolDispatcher
{
    Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct);
}

public sealed record LaunchResult(bool Succeeded, string Message);

public sealed record ConnectionProfileRequest(
    string Name,
    ProtocolType Protocol,
    string Host,
    int Port,
    string? Username,
    string? GroupPath,
    IEnumerable<string> Tags);
