using System.Data;
using System.Threading;

using GymTracker.Application.Admin.MeasurementTypes;

using Microsoft.Data.SqlClient;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlMeasurementTypeRepository : IMeasurementTypeRepository
{
    private static int _schemaInitialized;
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    private readonly string _connectionString;

    public SqlMeasurementTypeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<MeasurementTypesPageResponse> ListPageAsync(
        bool includeInactive = false,
        string? search = null,
        string? code = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        var normalizedSortBy = sortBy.Equals("name", StringComparison.OrdinalIgnoreCase)
            ? "[Name]"
            : sortBy.Equals("description", StringComparison.OrdinalIgnoreCase)
                ? "Description"
                : "[Key]";
        var normalizedSortDirection = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;

        var sql = $"""
            SELECT COUNT(1)
            FROM MeasurementTypes
            WHERE (@includeInactive = 1 OR IsDeleted = 0)
              AND (@search IS NULL
                   OR [Name] LIKE '%' + @search + '%'
                   OR [Key] LIKE '%' + @search + '%'
                   OR Description LIKE '%' + @search + '%')
              AND (@code IS NULL OR [Key] LIKE '%' + @code + '%');

            SELECT Id, [Key], [Name], Description, IsDeleted
            FROM MeasurementTypes
            WHERE (@includeInactive = 1 OR IsDeleted = 0)
              AND (@search IS NULL
                   OR [Name] LIKE '%' + @search + '%'
                   OR [Key] LIKE '%' + @search + '%'
                   OR Description LIKE '%' + @search + '%')
              AND (@code IS NULL OR [Key] LIKE '%' + @code + '%')
            ORDER BY {normalizedSortBy} {normalizedSortDirection}, Id {normalizedSortDirection}
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@includeInactive", includeInactive ? 1 : 0);
        command.Parameters.AddWithValue("@search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());
        command.Parameters.AddWithValue("@code", string.IsNullOrWhiteSpace(code) ? DBNull.Value : code.Trim());
        command.Parameters.AddWithValue("@offset", (normalizedPage - 1) * normalizedPageSize);
        command.Parameters.AddWithValue("@pageSize", normalizedPageSize);

        var totalCount = 0;
        var results = new List<MeasurementTypeResponse>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            totalCount = reader.GetInt32(0);
        }

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(Map(reader));
            }
        }

        return new MeasurementTypesPageResponse(results, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Key], [Name], Description
            FROM MeasurementTypes
            WHERE IsDeleted = 0
            ORDER BY [Key], Id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        var results = new List<AssignableMeasurementTypeResponse>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AssignableMeasurementTypeResponse(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return results;
    }

    public async Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Key], [Name], Description, IsDeleted
            FROM MeasurementTypes
            WHERE Id = @id AND IsDeleted = 0;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        var code = RequiredUpperCode(request.Code, nameof(request.Code));
        var name = string.IsNullOrWhiteSpace(request.Name) ? code : request.Name.Trim();
        var description = OptionalTrimmed(request.Description);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var hasConflict = await HasActiveKeyConflictAsync(connection, code, excludingId: null, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid().ToString("N");

        const string insertSql = """
            INSERT INTO MeasurementTypes (Id, [Key], [Name], Unit, DataType, Category, Description, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt)
            VALUES (@Id, @Key, @Name, 'count', 'integer', 'general', @Description, 1, 0, @CreatedAt, @UpdatedAt, NULL);
            """;

        await using var command = new SqlCommand(insertSql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Key", code);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", now);
        command.Parameters.AddWithValue("@UpdatedAt", now);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return new MeasurementTypeResponse(id, code, name, description, false);
    }

    public async Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var existing = await GetByIdInternalAsync(connection, id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var code = RequiredUpperCode(request.Code, nameof(request.Code));
        var name = string.IsNullOrWhiteSpace(request.Name) ? code : request.Name.Trim();
        var description = OptionalTrimmed(request.Description);

        var hasConflict = await HasActiveKeyConflictAsync(connection, code, id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        const string updateSql = """
            UPDATE MeasurementTypes
            SET [Key] = @Key,
                [Name] = @Name,
                Description = @Description,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(updateSql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Key", code);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return new MeasurementTypeResponse(id, code, name, description, existing.IsDeleted);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var existing = await GetByIdInternalAsync(connection, id, cancellationToken, includeDeleted: true);
        if (existing is null)
        {
            return false;
        }

        var updated = existing with { IsDeleted = true };

        const string sql = """
            UPDATE MeasurementTypes
            SET Active = 0,
                IsDeleted = 1,
                UpdatedAt = @UpdatedAt,
                DeletedAt = @DeletedAt
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);
        command.Parameters.AddWithValue("@DeletedAt", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    public async Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var existing = await GetByIdInternalAsync(connection, id, cancellationToken, includeDeleted: true);
        if (existing is null)
        {
            return null;
        }

        var hasConflict = await HasActiveKeyConflictAsync(connection, existing.Code, id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = existing with { IsDeleted = false };

        const string sql = """
            UPDATE MeasurementTypes
            SET Active = 1,
                IsDeleted = 0,
                UpdatedAt = @UpdatedAt,
                DeletedAt = NULL
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return updated;
    }

    private async Task<MeasurementTypeResponse?> GetByIdInternalAsync(SqlConnection connection, string id, CancellationToken cancellationToken, bool includeDeleted = false)
    {
        const string sql = """
            SELECT Id, [Key], [Name], Description, IsDeleted
            FROM MeasurementTypes
            WHERE Id = @id AND (@includeDeleted = 1 OR IsDeleted = 0);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@includeDeleted", includeDeleted ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    private static async Task<bool> HasActiveKeyConflictAsync(SqlConnection connection, string key, string? excludingId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM MeasurementTypes
            WHERE IsDeleted = 0
              AND UPPER([Key]) = UPPER(@key)
              AND (@excludingId IS NULL OR Id <> @excludingId);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@excludingId", (object?)excludingId ?? DBNull.Value);

        var count = (int)await command.ExecuteScalarAsync(cancellationToken);
        return count > 0;
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

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                IF OBJECT_ID(N'MeasurementTypes', N'U') IS NULL
                BEGIN
                    CREATE TABLE MeasurementTypes (
                        Id NVARCHAR(32) NOT NULL PRIMARY KEY,
                        [Key] NVARCHAR(100) NOT NULL,
                        [Name] NVARCHAR(200) NOT NULL,
                        Unit NVARCHAR(50) NOT NULL,
                        DataType NVARCHAR(50) NOT NULL,
                        Category NVARCHAR(50) NOT NULL,
                        Description NVARCHAR(MAX) NULL,
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
                    WHERE name = N'IX_MeasurementTypes_Key_Active'
                      AND object_id = OBJECT_ID(N'MeasurementTypes')
                )
                BEGIN
                    CREATE UNIQUE INDEX IX_MeasurementTypes_Key_Active
                    ON MeasurementTypes([Key])
                    WHERE IsDeleted = 0;
                END;
                """;

            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            Volatile.Write(ref _schemaInitialized, 1);
        }
        finally
        {
            SchemaLock.Release();
        }
    }

    private static MeasurementTypeResponse Map(SqlDataReader reader)
    {
        return new MeasurementTypeResponse(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetBoolean(4));
    }

    private static string RequiredTrimmed(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        return value.Trim();
    }

    private static string RequiredUpperCode(string? value, string paramName)
    {
        var trimmed = RequiredTrimmed(value, paramName);
        return trimmed.ToUpperInvariant().Replace(' ', '_');
    }

    private static string? OptionalTrimmed(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
