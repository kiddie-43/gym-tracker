using System.Data;
using System.Text.Json;
using System.Threading;

using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;

using Microsoft.Data.SqlClient;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlMuscleRepository : IMuscleRepository
{
    private static int _schemaInitialized;
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    private readonly string _connectionString;

    public SqlMuscleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Muscle?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Name], Code, Description, MuscleGroupIds, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
            FROM Muscles
            WHERE Id = @id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Name], Code, Description, MuscleGroupIds, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
            FROM Muscles
            WHERE (@includeDeleted = 1 OR IsDeleted = 0);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@includeDeleted", includeDeleted ? 1 : 0);

        var rows = new List<Muscle>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(Map(reader));
        }

        return rows;
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, string? excludingId = null, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(1)
            FROM Muscles
            WHERE IsDeleted = 0
              AND UPPER(Code) = UPPER(@code)
              AND (@excludingId IS NULL OR Id <> @excludingId);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@excludingId", (object?)excludingId ?? DBNull.Value);

        var count = (int)await command.ExecuteScalarAsync(cancellationToken);
        return count > 0;
    }

    public async Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            MERGE Muscles AS target
            USING (SELECT @Id AS Id) AS source
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    [Name] = @Name,
                    Code = @Code,
                    Description = @Description,
                    MuscleGroupIds = @MuscleGroupIds,
                    Active = @Active,
                    IsDeleted = @IsDeleted,
                    CreatedAt = @CreatedAt,
                    UpdatedAt = @UpdatedAt,
                    DeletedAt = @DeletedAt
            WHEN NOT MATCHED THEN
                INSERT (Id, [Name], Code, Description, MuscleGroupIds, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt)
                VALUES (@Id, @Name, @Code, @Description, @MuscleGroupIds, @Active, @IsDeleted, @CreatedAt, @UpdatedAt, @DeletedAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        AddRowParameters(command, entity);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            UPDATE Muscles
            SET Active = 0,
                IsDeleted = 1,
                UpdatedAt = @now,
                DeletedAt = @now
            WHERE Id = @id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@now", now);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public async Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            UPDATE Muscles
            SET Active = 1,
                IsDeleted = 0,
                UpdatedAt = @now,
                DeletedAt = NULL
            WHERE Id = @id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
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
                IF OBJECT_ID(N'Muscles', N'U') IS NULL
                BEGIN
                    CREATE TABLE Muscles (
                        Id NVARCHAR(32) NOT NULL PRIMARY KEY,
                        [Name] NVARCHAR(200) NOT NULL,
                        Code NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(MAX) NULL,
                        MuscleGroupIds NVARCHAR(MAX) NOT NULL,
                        Active BIT NOT NULL,
                        IsDeleted BIT NOT NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        DeletedAt DATETIMEOFFSET NULL
                    );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Muscles_Code_Active'
                      AND object_id = OBJECT_ID(N'Muscles')
                )
                BEGIN
                    CREATE UNIQUE INDEX IX_Muscles_Code_Active
                    ON Muscles(Code)
                    WHERE IsDeleted = 0;
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

    private static Muscle Map(SqlDataReader reader)
    {
        var id = reader.GetString(0);
        var name = reader.GetString(1);
        var code = reader.GetString(2);
        var description = reader.IsDBNull(3) ? null : reader.GetString(3);
        var groupIds = DeserializeMuscleGroupIds(reader.GetString(4));
        var active = reader.GetBoolean(5);
        var isDeleted = reader.GetBoolean(6);
        var createdAt = reader.GetDateTimeOffset(7);
        var deletedAt = reader.IsDBNull(9) ? (DateTimeOffset?)null : reader.GetDateTimeOffset(9);

        var entity = new Muscle
        {
            Id = id,
            CreatedAt = createdAt,
        };

        entity.Update(name, code, description, groupIds, active);
        if (isDeleted)
        {
            entity.SoftDelete(deletedAt ?? DateTimeOffset.UtcNow);
        }

        return entity;
    }

    private static string SerializeMuscleGroupIds(IReadOnlyCollection<string> ids)
    {
        return JsonSerializer.Serialize(ids);
    }

    private static IReadOnlyCollection<string> DeserializeMuscleGroupIds(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new[] { "general" };
        }

        var ids = JsonSerializer.Deserialize<string[]>(value) ?? Array.Empty<string>();
        return ids.Length == 0 ? new[] { "general" } : ids;
    }

    private static void AddRowParameters(SqlCommand command, Muscle entity)
    {
        command.Parameters.AddWithValue("@Id", entity.Id);
        command.Parameters.AddWithValue("@Name", entity.Name);
        command.Parameters.AddWithValue("@Code", entity.Code);
        command.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@MuscleGroupIds", SerializeMuscleGroupIds(entity.MuscleGroupIds));
        command.Parameters.AddWithValue("@Active", entity.Active);
        command.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
        command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);
        command.Parameters.AddWithValue("@DeletedAt", (object?)entity.DeletedAt ?? DBNull.Value);
    }
}
