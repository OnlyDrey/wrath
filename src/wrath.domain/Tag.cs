namespace Wrath.Domain;

public sealed record Tag
{
    public string Value { get; }

    public Tag(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 24) throw new DomainValidationException("Tag length must be between 2 and 24.");
        Value = normalized;
    }
}
