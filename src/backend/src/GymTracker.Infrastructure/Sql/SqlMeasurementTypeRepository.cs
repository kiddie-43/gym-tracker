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

        var normalizedSortBy = sortBy.Equals("category", StringComparison.OrdinalIgnoreCase)
            ? "Category"
            : sortBy.Equals("description", StringComparison.OrdinalIgnoreCase)
                ? "Description"
                : sortBy.Equals("code", StringComparison.OrdinalIgnoreCase)
                    ? "[Key]"
            : sortBy.Equals("key", StringComparison.OrdinalIgnoreCase)
                ? "[Key]"
                : "[Name]";
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
                   OR Category LIKE '%' + @search + '%'
                   OR Description LIKE '%' + @search + '%')
              AND (@code IS NULL OR [Key] LIKE '%' + @code + '%');

            SELECT Id, [Key], [Name], Unit, DataType, Category, Description, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
            FROM MeasurementTypes
            WHERE (@includeInactive = 1 OR IsDeleted = 0)
              AND (@search IS NULL
                   OR [Name] LIKE '%' + @search + '%'
                   OR [Key] LIKE '%' + @search + '%'
                   OR Category LIKE '%' + @search + '%'
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
            SELECT Id, [Key], [Name], Unit, DataType, Category
            FROM MeasurementTypes
            WHERE IsDeleted = 0
            ORDER BY [Name], Id;
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
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)));
        }

        return results;
    }

    public async Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Key], [Name], Unit, DataType, Category, Description, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
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

        var normalized = SanitizeRequest(request);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var hasConflict = await HasActiveKeyConflictAsync(connection, normalized.Key!, excludingId: null, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var now = DateTimeOffset.UtcNow;
        var row = new MeasurementTypeResponse(
            Id: Guid.NewGuid().ToString("N"),
            Key: normalized.Key!,
            Name: normalized.Name!,
            Unit: normalized.Unit!,
            DataType: normalized.DataType!,
            Category: normalized.Category!,
            Description: normalized.Description,
            Active: normalized.Active,
            IsDeleted: false,
            CreatedAt: now,
            UpdatedAt: now,
            DeletedAt: null);

        const string insertSql = """
            INSERT INTO MeasurementTypes (Id, [Key], [Name], Unit, DataType, Category, Description, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt)
            VALUES (@Id, @Key, @Name, @Unit, @DataType, @Category, @Description, @Active, @IsDeleted, @CreatedAt, @UpdatedAt, @DeletedAt);
            """;

        await using var command = new SqlCommand(insertSql, connection);
        AddRowParameters(command, row);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return row;
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

        var normalized = SanitizeRequest(request);

        var hasConflict = await HasActiveKeyConflictAsync(connection, normalized.Key!, id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = existing with
        {
            Key = normalized.Key!,
            Name = normalized.Name!,
            Unit = normalized.Unit!,
            DataType = normalized.DataType!,
            Category = normalized.Category!,
            Description = normalized.Description,
            Active = normalized.Active,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        const string updateSql = """
            UPDATE MeasurementTypes
            SET [Key] = @Key,
                [Name] = @Name,
                Unit = @Unit,
                DataType = @DataType,
                Category = @Category,
                Description = @Description,
                Active = @Active,
                IsDeleted = @IsDeleted,
                CreatedAt = @CreatedAt,
                UpdatedAt = @UpdatedAt,
                DeletedAt = @DeletedAt
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(updateSql, connection);
        AddRowParameters(command, updated);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return updated;
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

        var updated = existing with
        {
            Active = false,
            IsDeleted = true,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = DateTimeOffset.UtcNow,
        };

        const string sql = """
            UPDATE MeasurementTypes
            SET Active = @Active,
                IsDeleted = @IsDeleted,
                UpdatedAt = @UpdatedAt,
                DeletedAt = @DeletedAt
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Active", updated.Active);
        command.Parameters.AddWithValue("@IsDeleted", updated.IsDeleted);
        command.Parameters.AddWithValue("@UpdatedAt", updated.UpdatedAt);
        command.Parameters.AddWithValue("@DeletedAt", (object?)updated.DeletedAt ?? DBNull.Value);

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

        var hasConflict = await HasActiveKeyConflictAsync(connection, existing.Key, id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = existing with
        {
            Active = true,
            IsDeleted = false,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = null,
        };

        const string sql = """
            UPDATE MeasurementTypes
            SET Active = @Active,
                IsDeleted = @IsDeleted,
                UpdatedAt = @UpdatedAt,
                DeletedAt = @DeletedAt
            WHERE Id = @Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Active", updated.Active);
        command.Parameters.AddWithValue("@IsDeleted", updated.IsDeleted);
        command.Parameters.AddWithValue("@UpdatedAt", updated.UpdatedAt);
        command.Parameters.AddWithValue("@DeletedAt", DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return updated;
    }

    private async Task<MeasurementTypeResponse?> GetByIdInternalAsync(SqlConnection connection, string id, CancellationToken cancellationToken, bool includeDeleted = false)
    {
        const string sql = """
            SELECT Id, [Key], [Name], Unit, DataType, Category, Description, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
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
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.GetBoolean(7),
            reader.GetBoolean(8),
            reader.GetDateTimeOffset(9),
            reader.GetDateTimeOffset(10),
            reader.IsDBNull(11) ? null : reader.GetDateTimeOffset(11));
    }

    private static void AddRowParameters(SqlCommand command, MeasurementTypeResponse row)
    {
        command.Parameters.AddWithValue("@Id", row.Id);
        command.Parameters.AddWithValue("@Key", row.Key);
        command.Parameters.AddWithValue("@Name", row.Name);
        command.Parameters.AddWithValue("@Unit", row.Unit);
        command.Parameters.AddWithValue("@DataType", row.DataType);
        command.Parameters.AddWithValue("@Category", row.Category);
        command.Parameters.AddWithValue("@Description", (object?)row.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Active", row.Active);
        command.Parameters.AddWithValue("@IsDeleted", row.IsDeleted);
        command.Parameters.AddWithValue("@CreatedAt", row.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", row.UpdatedAt);
        command.Parameters.AddWithValue("@DeletedAt", (object?)row.DeletedAt ?? DBNull.Value);
    }

    private static UpsertMeasurementTypeRequest SanitizeRequest(UpsertMeasurementTypeRequest request)
    {
        var normalizedName = RequiredTrimmed(request.Name, nameof(request.Name));
        var inferred = InferDefaults(normalizedName);
        var normalizedKey = string.IsNullOrWhiteSpace(request.Key)
            ? inferred.Key
            : RequiredUpperCode(request.Key, nameof(request.Key));

        var legacyHasFields = request.Fields is { Count: > 0 };
        var normalizedUnit = string.IsNullOrWhiteSpace(request.Unit)
            ? (legacyHasFields ? "count" : inferred.Unit)
            : RequiredTrimmed(request.Unit, nameof(request.Unit));

        var normalizedDataType = string.IsNullOrWhiteSpace(request.DataType)
            ? (legacyHasFields ? "integer" : inferred.DataType)
            : RequiredTrimmed(request.DataType, nameof(request.DataType)).ToLowerInvariant();

        var normalizedCategory = string.IsNullOrWhiteSpace(request.Category)
            ? (legacyHasFields ? "general" : inferred.Category)
            : RequiredTrimmed(request.Category, nameof(request.Category)).ToLowerInvariant();

        return request with
        {
            Key = normalizedKey,
            Name = normalizedName,
            Unit = normalizedUnit,
            DataType = normalizedDataType,
            Category = normalizedCategory,
            Description = OptionalTrimmed(request.Description),
        };
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

    private static string NormalizeCode(string value)
    {
        var upper = value.Trim().ToUpperInvariant();
        var normalized = upper.Replace(' ', '_');
        return string.IsNullOrWhiteSpace(normalized) ? "MEASUREMENT_TYPE" : normalized;
    }

    private static (string Key, string Unit, string DataType, string Category) InferDefaults(string normalizedName)
    {
        var lower = normalizedName.ToLowerInvariant();

        if (lower.Contains("kilo") || lower.Contains("peso") || lower.Contains("kg"))
        {
            return ("PESO", "kg", "decimal", "strength");
        }

        if (lower.Contains("rep"))
        {
            return ("REPETICIONES", "reps", "integer", "strength");
        }

        if (lower.Contains("dist") || lower.Contains("metro") || lower.Contains("km"))
        {
            return ("DISTANCIA", "km", "decimal", "cardio");
        }

        return (NormalizeCode(normalizedName), "count", "integer", "general");
    }
}
