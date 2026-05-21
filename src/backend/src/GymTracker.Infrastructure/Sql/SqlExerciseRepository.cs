using System.Data;
using System.Text.Json;
using System.Threading;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

using Microsoft.Data.SqlClient;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlExerciseRepository : IExerciseRepository
{
    private static int _schemaInitialized;
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    private readonly string _connectionString;

    public SqlExerciseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Name], Code, Description, Category, Difficulty, MeasurementTypeId, MeasurementTypeCode,
                   MuscleGroupIds, MediaJson, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
            FROM Exercises
            WHERE Id = @id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        ExerciseRow? row = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            row = ReadExerciseRow(reader);
        }

        var primary = await LoadMuscleRefsAsync(connection, "ExercisePrimaryMuscles", new[] { row!.Id }, cancellationToken);
        var secondary = await LoadMuscleRefsAsync(connection, "ExerciseSecondaryMuscles", new[] { row.Id }, cancellationToken);
        var measurements = await LoadMeasurementRefsAsync(connection, new[] { row.Id }, cancellationToken);

        return Map(row, primary, secondary, measurements);
    }

    public async Task<IReadOnlyCollection<Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT Id, [Name], Code, Description, Category, Difficulty, MeasurementTypeId, MeasurementTypeCode,
                   MuscleGroupIds, MediaJson, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt
            FROM Exercises
            WHERE (@includeDeleted = 1 OR IsDeleted = 0);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@includeDeleted", includeDeleted ? 1 : 0);

        var rows = new List<ExerciseRow>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(ReadExerciseRow(reader));
            }
        }

        var ids = rows.Select(row => row.Id).ToArray();
        var primary = await LoadMuscleRefsAsync(connection, "ExercisePrimaryMuscles", ids, cancellationToken);
        var secondary = await LoadMuscleRefsAsync(connection, "ExerciseSecondaryMuscles", ids, cancellationToken);
        var measurements = await LoadMeasurementRefsAsync(connection, ids, cancellationToken);

        return rows.Select(row => Map(row, primary, secondary, measurements)).ToArray();
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(1)
            FROM Exercises
            WHERE IsDeleted = 0
              AND Active = 1
              AND UPPER(Code) = UPPER(@code)
              AND (@excludeExerciseId IS NULL OR Id <> @excludeExerciseId);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@excludeExerciseId", (object?)excludeExerciseId ?? DBNull.Value);

        var count = (int)await command.ExecuteScalarAsync(cancellationToken);
        return count > 0;
    }

    public async Task SaveAsync(Exercise entity, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string upsertSql = """
            MERGE Exercises AS target
            USING (SELECT @Id AS Id) AS source
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    [Name] = @Name,
                    Code = @Code,
                    Description = @Description,
                    Category = @Category,
                    Difficulty = @Difficulty,
                    MeasurementTypeId = @MeasurementTypeId,
                    MeasurementTypeCode = @MeasurementTypeCode,
                    MuscleGroupIds = @MuscleGroupIds,
                    MediaJson = @MediaJson,
                    Active = @Active,
                    IsDeleted = @IsDeleted,
                    CreatedAt = @CreatedAt,
                    UpdatedAt = @UpdatedAt,
                    DeletedAt = @DeletedAt
            WHEN NOT MATCHED THEN
                INSERT (Id, [Name], Code, Description, Category, Difficulty, MeasurementTypeId, MeasurementTypeCode,
                        MuscleGroupIds, MediaJson, Active, IsDeleted, CreatedAt, UpdatedAt, DeletedAt)
                VALUES (@Id, @Name, @Code, @Description, @Category, @Difficulty, @MeasurementTypeId, @MeasurementTypeCode,
                        @MuscleGroupIds, @MediaJson, @Active, @IsDeleted, @CreatedAt, @UpdatedAt, @DeletedAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var command = new SqlCommand(upsertSql, connection, (SqlTransaction)transaction))
        {
            AddRowParameters(command, entity);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await ReplaceMuscleRefsAsync(connection, (SqlTransaction)transaction, "ExercisePrimaryMuscles", entity.Id, entity.PrimaryMuscleIds, cancellationToken);
        await ReplaceMuscleRefsAsync(connection, (SqlTransaction)transaction, "ExerciseSecondaryMuscles", entity.Id, entity.SecondaryMuscleIds, cancellationToken);
        await ReplaceMeasurementRefsAsync(connection, (SqlTransaction)transaction, entity.Id, entity.MeasurementTypeIds, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        const string sql = """
            UPDATE Exercises
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
            UPDATE Exercises
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
                IF OBJECT_ID(N'Exercises', N'U') IS NULL
                BEGIN
                    CREATE TABLE Exercises (
                        Id NVARCHAR(32) NOT NULL PRIMARY KEY,
                        [Name] NVARCHAR(200) NOT NULL,
                        Code NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(MAX) NULL,
                        Category NVARCHAR(100) NOT NULL,
                        Difficulty NVARCHAR(100) NOT NULL,
                        MeasurementTypeId NVARCHAR(32) NOT NULL,
                        MeasurementTypeCode NVARCHAR(100) NOT NULL,
                        MuscleGroupIds NVARCHAR(MAX) NOT NULL,
                        MediaJson NVARCHAR(MAX) NOT NULL,
                        Active BIT NOT NULL,
                        IsDeleted BIT NOT NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        DeletedAt DATETIMEOFFSET NULL
                    );
                END;

                IF OBJECT_ID(N'ExercisePrimaryMuscles', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExercisePrimaryMuscles (
                        ExerciseId NVARCHAR(32) NOT NULL,
                        MuscleRef NVARCHAR(100) NOT NULL,
                        SortOrder INT NOT NULL,
                        CONSTRAINT PK_ExercisePrimaryMuscles PRIMARY KEY (ExerciseId, MuscleRef)
                    );
                    CREATE INDEX IX_ExercisePrimaryMuscles_ExerciseId ON ExercisePrimaryMuscles(ExerciseId);
                END;

                IF OBJECT_ID(N'ExerciseSecondaryMuscles', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExerciseSecondaryMuscles (
                        ExerciseId NVARCHAR(32) NOT NULL,
                        MuscleRef NVARCHAR(100) NOT NULL,
                        SortOrder INT NOT NULL,
                        CONSTRAINT PK_ExerciseSecondaryMuscles PRIMARY KEY (ExerciseId, MuscleRef)
                    );
                    CREATE INDEX IX_ExerciseSecondaryMuscles_ExerciseId ON ExerciseSecondaryMuscles(ExerciseId);
                END;

                IF OBJECT_ID(N'ExerciseMeasurementTypes', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExerciseMeasurementTypes (
                        ExerciseId NVARCHAR(32) NOT NULL,
                        MeasurementTypeId NVARCHAR(32) NOT NULL,
                        SortOrder INT NOT NULL,
                        CONSTRAINT PK_ExerciseMeasurementTypes PRIMARY KEY (ExerciseId, MeasurementTypeId)
                    );
                    CREATE INDEX IX_ExerciseMeasurementTypes_ExerciseId ON ExerciseMeasurementTypes(ExerciseId);
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Exercises_Code_Active'
                      AND object_id = OBJECT_ID(N'Exercises')
                )
                BEGIN
                    CREATE UNIQUE INDEX IX_Exercises_Code_Active
                    ON Exercises(Code)
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

    private static ExerciseRow ReadExerciseRow(SqlDataReader reader)
    {
        return new ExerciseRow(
            Id: reader.GetString(0),
            Name: reader.GetString(1),
            Code: reader.GetString(2),
            Description: reader.IsDBNull(3) ? null : reader.GetString(3),
            Category: reader.GetString(4),
            Difficulty: reader.GetString(5),
            MeasurementTypeId: reader.GetString(6),
            MeasurementTypeCode: reader.GetString(7),
            MuscleGroupIdsJson: reader.GetString(8),
            MediaJson: reader.GetString(9),
            Active: reader.GetBoolean(10),
            IsDeleted: reader.GetBoolean(11),
            CreatedAt: reader.GetDateTimeOffset(12),
            UpdatedAt: reader.GetDateTimeOffset(13),
            DeletedAt: reader.IsDBNull(14) ? null : reader.GetDateTimeOffset(14));
    }

    private static Exercise Map(
        ExerciseRow row,
        IReadOnlyDictionary<string, List<string>> primaryRefs,
        IReadOnlyDictionary<string, List<string>> secondaryRefs,
        IReadOnlyDictionary<string, List<string>> measurementRefs)
    {
        var primary = primaryRefs.TryGetValue(row.Id, out var primaryIds)
            ? primaryIds
            : new List<string>();
        var secondary = secondaryRefs.TryGetValue(row.Id, out var secondaryIds)
            ? secondaryIds
            : new List<string>();
        var measurements = measurementRefs.TryGetValue(row.Id, out var measurementTypeIds)
            ? measurementTypeIds
            : new List<string>();
        if (measurements.Count == 0 && !string.IsNullOrWhiteSpace(row.MeasurementTypeId))
        {
            measurements.Add(row.MeasurementTypeId);
        }
        var muscleGroups = DeserializeStringArray(row.MuscleGroupIdsJson, primary.ToArray());
        var media = DeserializeMedia(row.MediaJson);

        var entity = new Exercise
        {
            Id = row.Id,
            CreatedAt = row.CreatedAt,
        };

        entity.Update(
            row.Name,
            row.Code,
            row.Description,
            row.Category,
            row.Difficulty,
            measurements,
            row.MeasurementTypeCode,
            primary,
            secondary,
            muscleGroups,
            row.Active);

        entity.ReplaceMedia(media);

        if (row.IsDeleted)
        {
            entity.SoftDelete(row.DeletedAt ?? DateTimeOffset.UtcNow);
        }

        return entity;
    }

    private static string SerializeStringArray(IReadOnlyCollection<string> values)
    {
        return JsonSerializer.Serialize(values);
    }

    private static IReadOnlyCollection<string> DeserializeStringArray(string? json, IReadOnlyCollection<string> fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        var values = JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        return values.Length == 0 ? fallback : values;
    }

    private static string SerializeMedia(IReadOnlyCollection<ExerciseMedia> media)
    {
        return JsonSerializer.Serialize(media);
    }

    private static IReadOnlyCollection<ExerciseMedia> DeserializeMedia(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<ExerciseMedia>();
        }

        return JsonSerializer.Deserialize<ExerciseMedia[]>(json) ?? Array.Empty<ExerciseMedia>();
    }

    private static void AddRowParameters(SqlCommand command, Exercise entity)
    {
        command.Parameters.AddWithValue("@Id", entity.Id);
        command.Parameters.AddWithValue("@Name", entity.Name);
        command.Parameters.AddWithValue("@Code", entity.Code);
        command.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Category", entity.Category);
        command.Parameters.AddWithValue("@Difficulty", entity.Difficulty);
        command.Parameters.AddWithValue("@MeasurementTypeId", entity.MeasurementTypeId);
        command.Parameters.AddWithValue("@MeasurementTypeCode", entity.MeasurementTypeCode);
        command.Parameters.AddWithValue("@MuscleGroupIds", SerializeStringArray(entity.MuscleGroupIds));
        command.Parameters.AddWithValue("@MediaJson", SerializeMedia(entity.Media));
        command.Parameters.AddWithValue("@Active", entity.Active);
        command.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
        command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);
        command.Parameters.AddWithValue("@DeletedAt", (object?)entity.DeletedAt ?? DBNull.Value);
    }

    private static async Task ReplaceMuscleRefsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string tableName,
        string exerciseId,
        IReadOnlyCollection<string> refs,
        CancellationToken cancellationToken)
    {
        var deleteSql = $"DELETE FROM {tableName} WHERE ExerciseId = @exerciseId;";
        await using (var delete = new SqlCommand(deleteSql, connection, transaction))
        {
            delete.Parameters.AddWithValue("@exerciseId", exerciseId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        var index = 0;
        foreach (var item in refs)
        {
            var insertSql = $"INSERT INTO {tableName}(ExerciseId, MuscleRef, SortOrder) VALUES (@exerciseId, @muscleRef, @sortOrder);";
            await using var insert = new SqlCommand(insertSql, connection, transaction);
            insert.Parameters.AddWithValue("@exerciseId", exerciseId);
            insert.Parameters.AddWithValue("@muscleRef", item);
            insert.Parameters.AddWithValue("@sortOrder", index);
            await insert.ExecuteNonQueryAsync(cancellationToken);
            index++;
        }
    }

    private static async Task ReplaceMeasurementRefsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string exerciseId,
        IReadOnlyCollection<string> refs,
        CancellationToken cancellationToken)
    {
        const string deleteSql = "DELETE FROM ExerciseMeasurementTypes WHERE ExerciseId = @exerciseId;";
        await using (var delete = new SqlCommand(deleteSql, connection, transaction))
        {
            delete.Parameters.AddWithValue("@exerciseId", exerciseId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        var index = 0;
        foreach (var item in refs)
        {
            const string insertSql = "INSERT INTO ExerciseMeasurementTypes(ExerciseId, MeasurementTypeId, SortOrder) VALUES (@exerciseId, @measurementTypeId, @sortOrder);";
            await using var insert = new SqlCommand(insertSql, connection, transaction);
            insert.Parameters.AddWithValue("@exerciseId", exerciseId);
            insert.Parameters.AddWithValue("@measurementTypeId", item);
            insert.Parameters.AddWithValue("@sortOrder", index);
            await insert.ExecuteNonQueryAsync(cancellationToken);
            index++;
        }
    }

    private static async Task<Dictionary<string, List<string>>> LoadMeasurementRefsAsync(
        SqlConnection connection,
        IReadOnlyCollection<string> exerciseIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (exerciseIds.Count == 0)
        {
            return result;
        }

        var idParameters = exerciseIds.Select((_, index) => $"@id{index}").ToArray();
        var sql = $"SELECT ExerciseId, MeasurementTypeId FROM ExerciseMeasurementTypes WHERE ExerciseId IN ({string.Join(",", idParameters)}) ORDER BY SortOrder, MeasurementTypeId;";

        await using var command = new SqlCommand(sql, connection);

        var parameterIndex = 0;
        foreach (var id in exerciseIds)
        {
            command.Parameters.AddWithValue($"@id{parameterIndex}", id);
            parameterIndex++;
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var exerciseId = reader.GetString(0);
            var measurementTypeId = reader.GetString(1);

            if (!result.TryGetValue(exerciseId, out var refs))
            {
                refs = new List<string>();
                result[exerciseId] = refs;
            }

            refs.Add(measurementTypeId);
        }

        return result;
    }

    private static async Task<Dictionary<string, List<string>>> LoadMuscleRefsAsync(
        SqlConnection connection,
        string tableName,
        IReadOnlyCollection<string> exerciseIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (exerciseIds.Count == 0)
        {
            return result;
        }

        var idParameters = exerciseIds.Select((_, index) => $"@id{index}").ToArray();
        var sql = $"SELECT ExerciseId, MuscleRef FROM {tableName} WHERE ExerciseId IN ({string.Join(",", idParameters)}) ORDER BY SortOrder, MuscleRef;";

        await using var command = new SqlCommand(sql, connection);

        var parameterIndex = 0;
        foreach (var id in exerciseIds)
        {
            command.Parameters.AddWithValue($"@id{parameterIndex}", id);
            parameterIndex++;
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var exerciseId = reader.GetString(0);
            var muscleRef = reader.GetString(1);

            if (!result.TryGetValue(exerciseId, out var refs))
            {
                refs = new List<string>();
                result[exerciseId] = refs;
            }

            refs.Add(muscleRef);
        }

        return result;
    }

    private sealed record ExerciseRow(
        string Id,
        string Name,
        string Code,
        string? Description,
        string Category,
        string Difficulty,
        string MeasurementTypeId,
        string MeasurementTypeCode,
        string MuscleGroupIdsJson,
        string MediaJson,
        bool Active,
        bool IsDeleted,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? DeletedAt);
}
