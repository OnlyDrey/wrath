namespace Wrath.Security;

public interface ICredentialVaultAdapter
{
    Task<string> CreateCredentialReferenceAsync(string purpose, CancellationToken ct);
}

public sealed class WindowsVaultAdapter : ICredentialVaultAdapter
{
    public Task<string> CreateCredentialReferenceAsync(string purpose, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Purpose is required", nameof(purpose));
        // Safe extension point: implementation must use Windows.Security.Credentials on Windows host.
        return Task.FromResult($"vault://pending/{purpose.Trim().ToLowerInvariant()}/{Guid.NewGuid():N}");
    }
}
