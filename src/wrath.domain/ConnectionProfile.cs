namespace Wrath.Domain;

public sealed class ConnectionProfile
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public ProtocolType Protocol { get; private set; }
    public string Host { get; private set; }
    public int Port { get; private set; }
    public string? Username { get; private set; }
    public string? GroupPath { get; private set; }
    public bool IsFavorite { get; private set; }
    public IReadOnlyCollection<Tag> Tags => _tags;
    private readonly List<Tag> _tags = [];

    private ConnectionProfile() => (Id, Name, Host) = (Guid.Empty, string.Empty, string.Empty);

    public ConnectionProfile(Guid id, string name, ProtocolType protocol, string host, int port, string? username, string? groupPath, IEnumerable<Tag>? tags = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = Require(name, nameof(name), 3, 80);
        Protocol = protocol;
        Host = Require(host, nameof(host), 1, 255);
        Port = ValidatePort(protocol, port);
        Username = username?.Trim();
        GroupPath = groupPath?.Trim();
        if (tags is not null) _tags.AddRange(tags.DistinctBy(t => t.Value));
    }

    public void Update(string name, string host, int port, string? username, string? groupPath, IEnumerable<Tag> tags)
    {
        Name = Require(name, nameof(name), 3, 80);
        Host = Require(host, nameof(host), 1, 255);
        Port = ValidatePort(Protocol, port);
        Username = username?.Trim();
        GroupPath = groupPath?.Trim();

        _tags.Clear();
        _tags.AddRange(tags.DistinctBy(x => x.Value));
    }

    public void ToggleFavorite(bool favorite) => IsFavorite = favorite;

    private static string Require(string value, string field, int min, int max)
    {
        var v = value.Trim();
        if (v.Length < min || v.Length > max) throw new DomainValidationException($"{field} length must be between {min} and {max}.");
        return v;
    }

    private static int ValidatePort(ProtocolType protocol, int port)
    {
        if (port is <= 0 or > 65535) throw new DomainValidationException("Port must be in the range 1-65535.");
        return port;
    }
}
