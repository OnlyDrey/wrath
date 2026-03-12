using Microsoft.Data.Sqlite;

namespace Wrath.Infrastructure;

public sealed class SqliteInitializer
{
    private readonly SqliteOptions _options;

    public SqliteInitializer(SqliteOptions options) => _options = options;

    public async Task InitializeAsync(CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS connection_profiles (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    protocol INTEGER NOT NULL,
    host TEXT NOT NULL,
    port INTEGER NOT NULL,
    username TEXT NULL,
    group_path TEXT NULL,
    is_favorite INTEGER NOT NULL DEFAULT 0,
    tags TEXT NOT NULL DEFAULT ''
);
CREATE TABLE IF NOT EXISTS session_history (
    id TEXT PRIMARY KEY,
    connection_profile_id TEXT NOT NULL,
    protocol INTEGER NOT NULL,
    timestamp_utc TEXT NOT NULL,
    event_type INTEGER NOT NULL,
    message TEXT NOT NULL
);";
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
