using System.Text.Json;
using System.Threading;

using Microsoft.Data.SqlClient;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlDocumentStore
{
    private static int _schemaInitialized;
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SqlDocumentStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task UpsertUserAsync<T>(string module, string userId, string entityId, T payload, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        var json = JsonSerializer.Serialize(payload, _jsonOptions);

        const string sql = """
            MERGE UserJsonDocuments AS target
            USING (SELECT @Module AS ModuleName, @UserId AS UserId, @EntityId AS EntityId) AS source
            ON target.ModuleName = source.ModuleName AND target.UserId = source.UserId AND target.EntityId = source.EntityId
            WHEN MATCHED THEN
                UPDATE SET Payload = @Payload, UpdatedAt = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (ModuleName, UserId, EntityId, Payload, UpdatedAt)
                VALUES (@Module, @UserId, @EntityId, @Payload, @UpdatedAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Module", module);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@EntityId", entityId);
        command.Parameters.AddWithValue("@Payload", json);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T?> GetUserAsync<T>(string module, string userId, string entityId, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Payload
            FROM UserJsonDocuments
            WHERE ModuleName = @Module AND UserId = @UserId AND EntityId = @EntityId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Module", module);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@EntityId", entityId);

        var payload = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, _jsonOptions);
    }

    public async Task<IReadOnlyCollection<T>> ListUserAsync<T>(string module, string userId, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Payload
            FROM UserJsonDocuments
            WHERE ModuleName = @Module AND UserId = @UserId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Module", module);
        command.Parameters.AddWithValue("@UserId", userId);

        var results = new List<T>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var payload = reader.GetString(0);
            var item = JsonSerializer.Deserialize<T>(payload, _jsonOptions);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public async Task DeleteUserAsync(string module, string userId, string entityId, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            DELETE FROM UserJsonDocuments
            WHERE ModuleName = @Module AND UserId = @UserId AND EntityId = @EntityId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Module", module);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@EntityId", entityId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertDocumentAsync<T>(string scope, string key, T payload, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);
        var json = JsonSerializer.Serialize(payload, _jsonOptions);

        const string sql = """
            MERGE JsonDocuments AS target
            USING (SELECT @ScopeName AS ScopeName, @DocKey AS DocKey) AS source
            ON target.ScopeName = source.ScopeName AND target.DocKey = source.DocKey
            WHEN MATCHED THEN
                UPDATE SET Payload = @Payload, UpdatedAt = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (ScopeName, DocKey, Payload, UpdatedAt)
                VALUES (@ScopeName, @DocKey, @Payload, @UpdatedAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ScopeName", scope);
        command.Parameters.AddWithValue("@DocKey", key);
        command.Parameters.AddWithValue("@Payload", json);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T?> GetDocumentAsync<T>(string scope, string key, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Payload
            FROM JsonDocuments
            WHERE ScopeName = @ScopeName AND DocKey = @DocKey;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ScopeName", scope);
        command.Parameters.AddWithValue("@DocKey", key);

        var payload = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, _jsonOptions);
    }

    private async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref _schemaInitialized) == 1)
        {
            return;
        }

        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            if (Volatile.Read(ref _schemaInitialized) == 1)
            {
                return;
            }

            const string sql = """
                IF OBJECT_ID(N'UserJsonDocuments', N'U') IS NULL
                BEGIN
                    CREATE TABLE UserJsonDocuments (
                        ModuleName NVARCHAR(100) NOT NULL,
                        UserId NVARCHAR(128) NOT NULL,
                        EntityId NVARCHAR(128) NOT NULL,
                        Payload NVARCHAR(MAX) NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_UserJsonDocuments PRIMARY KEY (ModuleName, UserId, EntityId)
                    );
                END;

                IF OBJECT_ID(N'JsonDocuments', N'U') IS NULL
                BEGIN
                    CREATE TABLE JsonDocuments (
                        ScopeName NVARCHAR(100) NOT NULL,
                        DocKey NVARCHAR(256) NOT NULL,
                        Payload NVARCHAR(MAX) NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_JsonDocuments PRIMARY KEY (ScopeName, DocKey)
                    );
                END;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            Volatile.Write(ref _schemaInitialized, 1);
        }
        finally
        {
            SchemaLock.Release();
        }
    }
}
