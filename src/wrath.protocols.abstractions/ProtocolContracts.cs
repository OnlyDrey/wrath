using Microsoft.Extensions.Logging;
using Wrath.Domain;

namespace Wrath.Protocols.Abstractions;

public sealed record LaunchResult(bool Succeeded, string Message, string? Error = null);

public interface IProtocolDispatcher
{
    Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct);
}

public interface IProtocolProvider
{
    ProtocolType Protocol { get; }
    Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct);
}

public sealed class ProtocolDispatchService : IProtocolDispatcher
{
    private readonly IReadOnlyDictionary<ProtocolType, IProtocolProvider> _providers;
    private readonly ILogger<ProtocolDispatchService> _logger;

    public ProtocolDispatchService(IEnumerable<IProtocolProvider> providers, ILogger<ProtocolDispatchService> logger)
    {
        _providers = providers.ToDictionary(x => x.Protocol);
        _logger = logger;
    }

    public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct)
    {
        if (_providers.TryGetValue(profile.Protocol, out var provider))
        {
            return provider.LaunchAsync(profile, ct);
        }

        _logger.LogWarning("No protocol provider registered for protocol {Protocol}", profile.Protocol);
        return Task.FromResult(new LaunchResult(false, "Protocol provider not found.", $"No provider registered for protocol {profile.Protocol}."));
    }
}
