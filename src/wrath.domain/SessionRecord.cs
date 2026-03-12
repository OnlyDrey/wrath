namespace Wrath.Domain;

public sealed class SessionRecord
{
    public Guid Id { get; }
    public Guid ConnectionProfileId { get; }
    public ProtocolType Protocol { get; }
    public string Host { get; }
    public DateTimeOffset Timestamp { get; }
    public SessionEventType EventType { get; }
    public bool Succeeded { get; }
    public string Message { get; }
    public string? ErrorMessage { get; }

    public SessionRecord(
        Guid connectionProfileId,
        ProtocolType protocol,
        string host,
        SessionEventType eventType,
        bool succeeded,
        string message,
        string? errorMessage = null)
        : this(Guid.NewGuid(), connectionProfileId, protocol, host, DateTimeOffset.UtcNow, eventType, succeeded, message, errorMessage)
    {
    }

    public SessionRecord(
        Guid id,
        Guid connectionProfileId,
        ProtocolType protocol,
        string host,
        DateTimeOffset timestamp,
        SessionEventType eventType,
        bool succeeded,
        string message,
        string? errorMessage)
    {
        if (id == Guid.Empty) throw new DomainValidationException("Session id is required.");
        if (connectionProfileId == Guid.Empty) throw new DomainValidationException("Connection profile id is required.");
        if (string.IsNullOrWhiteSpace(host)) throw new DomainValidationException("Host is required.");
        if (string.IsNullOrWhiteSpace(message)) throw new DomainValidationException("Session message is required.");

        Id = id;
        ConnectionProfileId = connectionProfileId;
        Protocol = protocol;
        Host = host.Trim();
        Timestamp = timestamp;
        EventType = eventType;
        Succeeded = succeeded;
        Message = message.Trim();
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim();
    }
}
