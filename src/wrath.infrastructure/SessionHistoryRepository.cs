using Microsoft.Data.Sqlite;
using Wrath.Domain;

namespace Wrath.Infrastructure;

public sealed class SessionHistoryRepository : ISessionHistoryRepository
{
    private readonly SqliteOptions _options;

    public SessionHistoryRepository(SqliteOptions options) => _options = options;

    public async Task AddAsync(SessionRecord record, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO session_history
(id, connection_profile_id, protocol, host, timestamp_utc, event_type, succeeded, message, error_message)
VALUES($id,$connection,$protocol,$host,$timestamp,$event,$succeeded,$message,$error)";

        cmd.Parameters.AddWithValue("$id", record.Id.ToString());
        cmd.Parameters.AddWithValue("$connection", record.ConnectionProfileId.ToString());
        cmd.Parameters.AddWithValue("$protocol", (int)record.Protocol);
        cmd.Parameters.AddWithValue("$host", record.Host);
        cmd.Parameters.AddWithValue("$timestamp", record.Timestamp.ToString("O"));
        cmd.Parameters.AddWithValue("$event", (int)record.EventType);
        cmd.Parameters.AddWithValue("$succeeded", record.Succeeded ? 1 : 0);
        cmd.Parameters.AddWithValue("$message", record.Message);
        cmd.Parameters.AddWithValue("$error", (object?)record.ErrorMessage ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<SessionRecord>> GetRecentAsync(int take, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM session_history ORDER BY timestamp_utc DESC LIMIT $take";
        cmd.Parameters.AddWithValue("$take", take);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<SessionRecord>();
        while (await reader.ReadAsync(ct))
        {
            var item = new SessionRecord(
                Guid.Parse(reader.GetString(reader.GetOrdinal("id"))),
                Guid.Parse(reader.GetString(reader.GetOrdinal("connection_profile_id"))),
                (ProtocolType)reader.GetInt32(reader.GetOrdinal("protocol")),
                reader.GetString(reader.GetOrdinal("host")),
                DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("timestamp_utc"))),
                (SessionEventType)reader.GetInt32(reader.GetOrdinal("event_type")),
                reader.GetInt32(reader.GetOrdinal("succeeded")) == 1,
                reader.GetString(reader.GetOrdinal("message")),
                reader.IsDBNull(reader.GetOrdinal("error_message")) ? null : reader.GetString(reader.GetOrdinal("error_message")));
            list.Add(item);
        }

        return list;
    }
}
