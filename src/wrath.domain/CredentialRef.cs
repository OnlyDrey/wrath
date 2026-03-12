namespace Wrath.Domain;

public sealed record CredentialRef(string Provider, string Key)
{
    public static CredentialRef Create(string provider, string key)
    {
        if (string.IsNullOrWhiteSpace(provider)) throw new DomainValidationException("Credential provider is required.");
        if (string.IsNullOrWhiteSpace(key)) throw new DomainValidationException("Credential key is required.");
        return new CredentialRef(provider.Trim(), key.Trim());
    }
}
