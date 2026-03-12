using Microsoft.Data.Sqlite;
using Wrath.Domain;

namespace Wrath.Infrastructure;

public sealed class ConnectionProfileRepository : IConnectionProfileRepository
{
    private readonly SqliteOptions _options;

    public ConnectionProfileRepository(SqliteOptions options) => _options = options;

    public async Task AddAsync(ConnectionProfile profile, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO connection_profiles
(id, name, protocol, host, port, username, group_path, is_favorite, tags)
VALUES($id,$name,$protocol,$host,$port,$username,$group,$fav,$tags)";
        Bind(profile, cmd);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(ConnectionProfile profile, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE connection_profiles SET
name=$name, host=$host, port=$port, username=$username, group_path=$group, is_favorite=$fav, tags=$tags
WHERE id=$id";
        Bind(profile, cmd);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<ConnectionProfile?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM connection_profiles WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<ConnectionProfile>> SearchAsync(string? query, CancellationToken ct)
    {
        await using var conn = new SqliteConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        var text = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            cmd.CommandText = "SELECT * FROM connection_profiles ORDER BY name";
        }
        else
        {
            cmd.CommandText = "SELECT * FROM connection_profiles WHERE name LIKE $q OR host LIKE $q OR tags LIKE $q ORDER BY name";
            cmd.Parameters.AddWithValue("$q", $"%{text}%");
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<ConnectionProfile>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Map(reader));
        }

        return result;
    }

    private static ConnectionProfile Map(SqliteDataReader reader)
    {
        var tags = reader.GetString(reader.GetOrdinal("tags"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => new Tag(x));

        var profile = new ConnectionProfile(
            Guid.Parse(reader.GetString(reader.GetOrdinal("id"))),
            reader.GetString(reader.GetOrdinal("name")),
            (ProtocolType)reader.GetInt32(reader.GetOrdinal("protocol")),
            reader.GetString(reader.GetOrdinal("host")),
            reader.GetInt32(reader.GetOrdinal("port")),
            reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
            reader.IsDBNull(reader.GetOrdinal("group_path")) ? null : reader.GetString(reader.GetOrdinal("group_path")),
            tags);

        profile.ToggleFavorite(reader.GetInt32(reader.GetOrdinal("is_favorite")) == 1);
        return profile;
    }

    private static void Bind(ConnectionProfile profile, SqliteCommand cmd)
    {
        cmd.Parameters.AddWithValue("$id", profile.Id.ToString());
        cmd.Parameters.AddWithValue("$name", profile.Name);
        cmd.Parameters.AddWithValue("$protocol", (int)profile.Protocol);
        cmd.Parameters.AddWithValue("$host", profile.Host);
        cmd.Parameters.AddWithValue("$port", profile.Port);
        cmd.Parameters.AddWithValue("$username", (object?)profile.Username ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$group", (object?)profile.GroupPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$fav", profile.IsFavorite ? 1 : 0);
        cmd.Parameters.AddWithValue("$tags", string.Join(',', profile.Tags.Select(x => x.Value)));
    }
}
