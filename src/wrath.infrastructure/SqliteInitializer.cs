using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Wrath.Infrastructure;

public sealed class SqliteInitializer
{
    public const int CurrentSchemaVersion = 1;

    private readonly SqliteOptions _options;
    private readonly ILogger<SqliteInitializer> _logger;

    public SqliteInitializer(SqliteOptions options, ILogger<SqliteInitializer> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        _logger.LogInformation("Initializing SQLite database. DataSource={DataSource}", conn.DataSource);

        await EnsureMetadataTableAsync(conn, ct);
        var existingVersion = await GetSchemaVersionAsync(conn, ct);
        if (existingVersion < CurrentSchemaVersion)
        {
            await ApplyMigrationsAsync(conn, existingVersion, ct);
        }

        _logger.LogInformation("SQLite initialization complete. SchemaVersion={SchemaVersion}", CurrentSchemaVersion);
    }

    private static async Task EnsureMetadataTableAsync(SqliteConnection conn, CancellationToken ct)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS DatabaseMetadata (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    SchemaVersion INTEGER NOT NULL
);
INSERT INTO DatabaseMetadata(id, SchemaVersion)
SELECT 1, 0
WHERE NOT EXISTS (SELECT 1 FROM DatabaseMetadata WHERE id = 1);";
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> GetSchemaVersionAsync(SqliteConnection conn, CancellationToken ct)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT SchemaVersion FROM DatabaseMetadata WHERE id = 1";
        var value = await cmd.ExecuteScalarAsync(ct);
        return value is null ? 0 : Convert.ToInt32(value);
    }

    private async Task ApplyMigrationsAsync(SqliteConnection conn, int fromVersion, CancellationToken ct)
    {
        if (fromVersion < 1)
        {
            _logger.LogInformation("Applying migration 1: initial schema");
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
    host TEXT NOT NULL,
    timestamp_utc TEXT NOT NULL,
    event_type INTEGER NOT NULL,
    succeeded INTEGER NOT NULL,
    message TEXT NOT NULL,
    error_message TEXT NULL
);
CREATE INDEX IF NOT EXISTS idx_connection_profiles_name ON connection_profiles(name);
CREATE INDEX IF NOT EXISTS idx_connection_profiles_host ON connection_profiles(host);
CREATE INDEX IF NOT EXISTS idx_connection_profiles_tags ON connection_profiles(tags);
CREATE INDEX IF NOT EXISTS idx_session_history_timestamp ON session_history(timestamp_utc DESC);";
            await cmd.ExecuteNonQueryAsync(ct);

            var setVersion = conn.CreateCommand();
            setVersion.CommandText = "UPDATE DatabaseMetadata SET SchemaVersion = $version WHERE id = 1";
            setVersion.Parameters.AddWithValue("$version", CurrentSchemaVersion);
            await setVersion.ExecuteNonQueryAsync(ct);
        }
    }
}
