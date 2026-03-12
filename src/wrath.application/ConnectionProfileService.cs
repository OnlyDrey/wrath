using Wrath.Domain;

namespace Wrath.Application;

public sealed class ConnectionProfileService
{
    private readonly IConnectionProfileRepository _profiles;

    public ConnectionProfileService(IConnectionProfileRepository profiles) => _profiles = profiles;

    public async Task<Guid> CreateConnectionProfileAsync(ConnectionProfileRequest request, CancellationToken ct)
    {
        var profile = new ConnectionProfile(
            Guid.NewGuid(),
            request.Name,
            request.Protocol,
            request.Host,
            request.Port,
            request.Username,
            request.GroupPath,
            request.Tags.Select(t => new Tag(t)));

        await _profiles.AddAsync(profile, ct);
        return profile.Id;
    }

    public async Task UpdateConnectionProfileAsync(Guid id, ConnectionProfileRequest request, CancellationToken ct)
    {
        var existing = await _profiles.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Profile not found.");
        existing.Update(request.Name, request.Host, request.Port, request.Username, request.GroupPath, request.Tags.Select(t => new Tag(t)));
        await _profiles.UpdateAsync(existing, ct);
    }

    public Task<IReadOnlyList<ConnectionProfile>> SearchConnectionsAsync(string? query, CancellationToken ct)
        => _profiles.SearchAsync(query, ct);
}
