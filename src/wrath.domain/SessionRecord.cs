namespace Wrath.Domain;

public sealed class SessionRecord
{
    public Guid Id { get; }
    public Guid ConnectionProfileId { get; }
    public ProtocolType Protocol { get; }
    public DateTimeOffset Timestamp { get; }
    public SessionEventType EventType { get; }
    public string Message { get; }

    public SessionRecord(Guid connectionProfileId, ProtocolType protocol, SessionEventType eventType, string message)
        : this(Guid.NewGuid(), connectionProfileId, protocol, DateTimeOffset.UtcNow, eventType, message)
    {
    }

    public SessionRecord(Guid id, Guid connectionProfileId, ProtocolType protocol, DateTimeOffset timestamp, SessionEventType eventType, string message)
    {
        if (id == Guid.Empty) throw new DomainValidationException("Session id is required.");
        if (connectionProfileId == Guid.Empty) throw new DomainValidationException("Connection profile id is required.");
        if (string.IsNullOrWhiteSpace(message)) throw new DomainValidationException("Session message is required.");

        Id = id;
        ConnectionProfileId = connectionProfileId;
        Protocol = protocol;
        Timestamp = timestamp;
        EventType = eventType;
        Message = message.Trim();
    }
}
