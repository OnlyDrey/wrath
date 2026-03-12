using Wrath.Application;
using Wrath.Domain;

namespace Wrath.Protocols.Abstractions;

public interface IProtocolProvider
{
    ProtocolType Protocol { get; }
    Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct);
}

public sealed class ProtocolDispatchService : IProtocolDispatcher
{
    private readonly IReadOnlyDictionary<ProtocolType, IProtocolProvider> _providers;

    public ProtocolDispatchService(IEnumerable<IProtocolProvider> providers)
        => _providers = providers.ToDictionary(x => x.Protocol);

    public Task<LaunchResult> LaunchAsync(ConnectionProfile profile, CancellationToken ct)
    {
        if (_providers.TryGetValue(profile.Protocol, out var provider))
        {
            return provider.LaunchAsync(profile, ct);
        }

        return Task.FromResult(new LaunchResult(false, $"No provider registered for protocol {profile.Protocol}."));
    }
}
