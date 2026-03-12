namespace Wrath.Domain;

public interface IConnectionProfileRepository
{
    Task AddAsync(ConnectionProfile profile, CancellationToken ct);
    Task UpdateAsync(ConnectionProfile profile, CancellationToken ct);
    Task<ConnectionProfile?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ConnectionProfile>> SearchAsync(string? query, CancellationToken ct);
}

public interface ISessionHistoryRepository
{
    Task AddAsync(SessionRecord record, CancellationToken ct);
    Task<IReadOnlyList<SessionRecord>> GetRecentAsync(int take, CancellationToken ct);
}
