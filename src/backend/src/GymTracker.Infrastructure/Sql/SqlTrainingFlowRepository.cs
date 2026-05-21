using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Training;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlTrainingFlowRepository : ITrainingFlowRepository
{
    private readonly string _connectionString;

    public SqlTrainingFlowRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("Sql").GetValue<string>("ConnectionString")
            ?? throw new InvalidOperationException("Sql:ConnectionString is not configured.");
    }

    public async Task<TrainingFlowState?> GetActiveStateAsync(string userId, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        const string sql = """
            SELECT UserId, IsLocked, RoutineId, SessionId, ExerciseId, StepNode, LastUpdatedAt
            FROM TrainingFlowStatesV2
            WHERE UserId = @UserId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new TrainingFlowState
        {
            UserId = reader.GetString(0),
            IsLocked = reader.GetBoolean(1),
            RoutineId = reader.GetString(2),
            SessionId = reader.IsDBNull(3) ? null : reader.GetString(3),
            ExerciseId = reader.IsDBNull(4) ? null : reader.GetString(4),
            StepNode = reader.GetString(5),
            LastUpdatedAt = reader.GetFieldValue<DateTimeOffset>(6),
        };
    }

    public async Task SaveActiveStateAsync(TrainingFlowState state, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        const string sql = """
            MERGE TrainingFlowStatesV2 AS target
            USING (SELECT @UserId AS UserId) AS source
            ON target.UserId = source.UserId
            WHEN MATCHED THEN
                UPDATE SET
                    IsLocked = @IsLocked,
                    RoutineId = @RoutineId,
                    SessionId = @SessionId,
                    ExerciseId = @ExerciseId,
                    StepNode = @StepNode,
                    LastUpdatedAt = @LastUpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (UserId, IsLocked, RoutineId, SessionId, ExerciseId, StepNode, LastUpdatedAt)
                VALUES (@UserId, @IsLocked, @RoutineId, @SessionId, @ExerciseId, @StepNode, @LastUpdatedAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", state.UserId);
        command.Parameters.AddWithValue("@IsLocked", state.IsLocked);
        command.Parameters.AddWithValue("@RoutineId", state.RoutineId);
        command.Parameters.AddWithValue("@SessionId", (object?)state.SessionId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ExerciseId", (object?)state.ExerciseId ?? DBNull.Value);
        command.Parameters.AddWithValue("@StepNode", state.StepNode);
        command.Parameters.AddWithValue("@LastUpdatedAt", state.LastUpdatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ClearActiveStateAsync(string userId, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        const string sql = "DELETE FROM TrainingFlowStatesV2 WHERE UserId = @UserId;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveLogAsync(ExerciseTrainingLog log, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string upsertLogSql = """
            MERGE ExerciseTrainingLogsV2 AS target
            USING (SELECT @LogId AS LogId) AS source
            ON target.LogId = source.LogId
            WHEN MATCHED THEN
                UPDATE SET
                    UserId = @UserId,
                    RoutineId = @RoutineId,
                    SessionId = @SessionId,
                    ExerciseId = @ExerciseId,
                    Notes = @Notes,
                    UpdatedAt = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (LogId, UserId, RoutineId, SessionId, ExerciseId, Notes, CreatedAt, UpdatedAt)
                VALUES (@LogId, @UserId, @RoutineId, @SessionId, @ExerciseId, @Notes, @CreatedAt, @UpdatedAt);
            """;

        await using (var command = new SqlCommand(upsertLogSql, connection, (SqlTransaction)transaction))
        {
            command.Parameters.AddWithValue("@LogId", log.Id);
            command.Parameters.AddWithValue("@UserId", log.UserId);
            command.Parameters.AddWithValue("@RoutineId", log.RoutineId);
            command.Parameters.AddWithValue("@SessionId", log.SessionId);
            command.Parameters.AddWithValue("@ExerciseId", log.ExerciseId);
            command.Parameters.AddWithValue("@Notes", (object?)log.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", log.UpdatedAt);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        const string deleteChildrenSql = """
            DELETE FROM ExerciseTrainingLogSetsV2 WHERE LogId = @LogId;
            DELETE FROM ExerciseTrainingLogAttachmentsV2 WHERE LogId = @LogId;
            """;

        await using (var command = new SqlCommand(deleteChildrenSql, connection, (SqlTransaction)transaction))
        {
            command.Parameters.AddWithValue("@LogId", log.Id);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var setItem in log.PerformedSets)
        {
            const string insertSetSql = """
                INSERT INTO ExerciseTrainingLogSetsV2 (LogId, [Order], Repetitions, WeightKg)
                VALUES (@LogId, @Order, @Repetitions, @WeightKg);
                """;

            await using var command = new SqlCommand(insertSetSql, connection, (SqlTransaction)transaction);
            command.Parameters.AddWithValue("@LogId", log.Id);
            command.Parameters.AddWithValue("@Order", setItem.Order);
            command.Parameters.AddWithValue("@Repetitions", setItem.Repetitions);
            command.Parameters.AddWithValue("@WeightKg", setItem.WeightKg);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var attachment in log.Attachments)
        {
            const string insertAttachmentSql = """
                INSERT INTO ExerciseTrainingLogAttachmentsV2 (AttachmentId, LogId, AttachmentType, Url, UploadedAt)
                VALUES (@AttachmentId, @LogId, @AttachmentType, @Url, @UploadedAt);
                """;

            await using var command = new SqlCommand(insertAttachmentSql, connection, (SqlTransaction)transaction);
            command.Parameters.AddWithValue("@AttachmentId", attachment.Id);
            command.Parameters.AddWithValue("@LogId", log.Id);
            command.Parameters.AddWithValue("@AttachmentType", attachment.Type);
            command.Parameters.AddWithValue("@Url", attachment.Url);
            command.Parameters.AddWithValue("@UploadedAt", attachment.UploadedAt);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<ExerciseTrainingLog?> GetLogByIdAsync(string userId, string logId, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        const string sql = """
            SELECT LogId, UserId, RoutineId, SessionId, ExerciseId, Notes, CreatedAt, UpdatedAt
            FROM ExerciseTrainingLogsV2
            WHERE UserId = @UserId AND LogId = @LogId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@LogId", logId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var log = new ExerciseTrainingLog
        {
            Id = reader.GetString(0),
            UserId = reader.GetString(1),
            RoutineId = reader.GetString(2),
            SessionId = reader.GetString(3),
            ExerciseId = reader.GetString(4),
            Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(6),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(7),
            PerformedSets = new List<PerformedSet>(),
            Attachments = new List<TrainingAttachment>(),
        };

        log.PerformedSets.AddRange(await LoadSetsByLogAsync(connection, log.Id, cancellationToken));
        log.Attachments.AddRange(await LoadAttachmentsByLogAsync(connection, log.Id, cancellationToken));

        return log;
    }

    public async Task<IReadOnlyCollection<ExerciseTrainingLog>> ListLogsByExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        const string logsSql = """
            SELECT LogId, UserId, RoutineId, SessionId, ExerciseId, Notes, CreatedAt, UpdatedAt
            FROM ExerciseTrainingLogsV2
            WHERE UserId = @UserId AND ExerciseId = @ExerciseId
            ORDER BY CreatedAt DESC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var logs = new List<ExerciseTrainingLog>();

        await using (var command = new SqlCommand(logsSql, connection))
        {
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@ExerciseId", exerciseId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                logs.Add(new ExerciseTrainingLog
                {
                    Id = reader.GetString(0),
                    UserId = reader.GetString(1),
                    RoutineId = reader.GetString(2),
                    SessionId = reader.GetString(3),
                    ExerciseId = reader.GetString(4),
                    Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreatedAt = reader.GetFieldValue<DateTimeOffset>(6),
                    UpdatedAt = reader.GetFieldValue<DateTimeOffset>(7),
                    PerformedSets = new List<PerformedSet>(),
                    Attachments = new List<TrainingAttachment>(),
                });
            }
        }

        if (logs.Count == 0)
        {
            return logs;
        }

        var setsByLogId = await LoadSetsByExerciseAsync(connection, userId, exerciseId, cancellationToken);
        var attachmentsByLogId = await LoadAttachmentsByExerciseAsync(connection, userId, exerciseId, cancellationToken);

        foreach (var log in logs)
        {
            if (setsByLogId.TryGetValue(log.Id, out var sets))
            {
                log.PerformedSets.AddRange(sets);
            }

            if (attachmentsByLogId.TryGetValue(log.Id, out var attachments))
            {
                log.Attachments.AddRange(attachments);
            }
        }

        return logs;
    }

    private static async Task<List<PerformedSet>> LoadSetsByLogAsync(SqlConnection connection, string logId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT [Order], Repetitions, WeightKg
            FROM ExerciseTrainingLogSetsV2
            WHERE LogId = @LogId
            ORDER BY [Order];
            """;

        var sets = new List<PerformedSet>();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@LogId", logId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sets.Add(new PerformedSet
            {
                Order = reader.GetInt32(0),
                Repetitions = reader.GetInt32(1),
                WeightKg = reader.GetDecimal(2),
            });
        }

        return sets;
    }

    private static async Task<List<TrainingAttachment>> LoadAttachmentsByLogAsync(SqlConnection connection, string logId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT AttachmentId, AttachmentType, Url, UploadedAt
            FROM ExerciseTrainingLogAttachmentsV2
            WHERE LogId = @LogId;
            """;

        var attachments = new List<TrainingAttachment>();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@LogId", logId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            attachments.Add(new TrainingAttachment
            {
                Id = reader.GetString(0),
                Type = reader.GetString(1),
                Url = reader.GetString(2),
                UploadedAt = reader.GetFieldValue<DateTimeOffset>(3),
            });
        }

        return attachments;
    }

    private static async Task<Dictionary<string, List<PerformedSet>>> LoadSetsByExerciseAsync(
        SqlConnection connection,
        string userId,
        string exerciseId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT setEntry.LogId, setEntry.[Order], setEntry.Repetitions, setEntry.WeightKg
            FROM ExerciseTrainingLogSetsV2 setEntry
            INNER JOIN ExerciseTrainingLogsV2 logEntry ON logEntry.LogId = setEntry.LogId
            WHERE logEntry.UserId = @UserId AND logEntry.ExerciseId = @ExerciseId
            ORDER BY setEntry.LogId, setEntry.[Order];
            """;

        var result = new Dictionary<string, List<PerformedSet>>(StringComparer.Ordinal);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ExerciseId", exerciseId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var logId = reader.GetString(0);
            if (!result.TryGetValue(logId, out var entries))
            {
                entries = new List<PerformedSet>();
                result[logId] = entries;
            }

            entries.Add(new PerformedSet
            {
                Order = reader.GetInt32(1),
                Repetitions = reader.GetInt32(2),
                WeightKg = reader.GetDecimal(3),
            });
        }

        return result;
    }

    private static async Task<Dictionary<string, List<TrainingAttachment>>> LoadAttachmentsByExerciseAsync(
        SqlConnection connection,
        string userId,
        string exerciseId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT attachment.LogId, attachment.AttachmentId, attachment.AttachmentType, attachment.Url, attachment.UploadedAt
            FROM ExerciseTrainingLogAttachmentsV2 attachment
            INNER JOIN ExerciseTrainingLogsV2 logEntry ON logEntry.LogId = attachment.LogId
            WHERE logEntry.UserId = @UserId AND logEntry.ExerciseId = @ExerciseId
            ORDER BY attachment.LogId;
            """;

        var result = new Dictionary<string, List<TrainingAttachment>>(StringComparer.Ordinal);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ExerciseId", exerciseId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var logId = reader.GetString(0);
            if (!result.TryGetValue(logId, out var entries))
            {
                entries = new List<TrainingAttachment>();
                result[logId] = entries;
            }

            entries.Add(new TrainingAttachment
            {
                Id = reader.GetString(1),
                Type = reader.GetString(2),
                Url = reader.GetString(3),
                UploadedAt = reader.GetFieldValue<DateTimeOffset>(4),
            });
        }

        return result;
    }
}
