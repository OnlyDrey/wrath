using Microsoft.Extensions.Logging;
using Wrath.Domain;

namespace Wrath.Application;

public sealed class ConnectionProfileService
{
    private readonly IConnectionProfileRepository _profiles;
    private readonly ILogger<ConnectionProfileService> _logger;

    public ConnectionProfileService(IConnectionProfileRepository profiles, ILogger<ConnectionProfileService> logger)
    {
        _profiles = profiles;
        _logger = logger;
    }

    public async Task<OperationResult<Guid>> CreateConnectionProfileAsync(ConnectionProfileRequest request, CancellationToken ct)
    {
        try
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
            _logger.LogInformation("Connection profile created. ProfileId={ProfileId} Name={Name} Protocol={Protocol} Host={Host}",
                profile.Id, profile.Name, profile.Protocol, profile.Host);

            return OperationResult<Guid>.Success(profile.Id, "Connection profile created.");
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(ex, "Connection profile validation failed for Name={Name}", request.Name);
            return OperationResult<Guid>.Failure("Invalid connection profile.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection profile creation failed for Name={Name}", request.Name);
            return OperationResult<Guid>.Failure("Failed to create connection profile.", ex.Message);
        }
    }

    public async Task<OperationResult> UpdateConnectionProfileAsync(Guid id, ConnectionProfileRequest request, CancellationToken ct)
    {
        try
        {
            var existing = await _profiles.GetByIdAsync(id, ct);
            if (existing is null)
            {
                return OperationResult.Failure("Connection profile not found.");
            }

            existing.Update(
                request.Name,
                request.Host,
                request.Port,
                request.Username,
                request.GroupPath,
                request.Tags.Select(t => new Tag(t)));

            await _profiles.UpdateAsync(existing, ct);
            _logger.LogInformation("Connection profile updated. ProfileId={ProfileId} Name={Name}", existing.Id, existing.Name);
            return OperationResult.Success("Connection profile updated.");
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(ex, "Connection profile update validation failed. ProfileId={ProfileId}", id);
            return OperationResult.Failure("Invalid connection profile.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection profile update failed. ProfileId={ProfileId}", id);
            return OperationResult.Failure("Failed to update connection profile.", ex.Message);
        }
    }

    public async Task<IReadOnlyList<ConnectionProfile>> SearchConnectionsAsync(string? query, CancellationToken ct)
    {
        var normalized = query?.Trim();
        _logger.LogDebug("Searching profiles. Query={Query}", normalized);
        return await _profiles.SearchAsync(normalized, ct);
    }
}
