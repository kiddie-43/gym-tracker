using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Routines;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlRoutineAggregateRepository : IRoutineRepository
{
    private readonly string _connectionString;

    public SqlRoutineAggregateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("Sql").GetValue<string>("ConnectionString")
            ?? throw new InvalidOperationException("Sql:ConnectionString is not configured.");
    }

    public async Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        var routines = new Dictionary<string, Routine>(StringComparer.Ordinal);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string routineSql = """
            SELECT RoutineId, UserId, Title, Goal, IsDeleted, CreatedAt
            FROM RoutinesV2
            WHERE UserId = @UserId;
            """;

        await using (var command = new SqlCommand(routineSql, connection))
        {
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var routine = new Routine
                {
                    Id = reader.GetString(0),
                    UserId = reader.GetString(1),
                    Title = reader.GetString(2),
                    Goal = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = reader.GetFieldValue<DateTimeOffset>(5),
                };

                if (reader.GetBoolean(4))
                {
                    routine.Archive();
                }

                routines[routine.Id] = routine;
            }
        }

        if (routines.Count == 0)
        {
            return Array.Empty<Routine>();
        }

        var sessions = await LoadSessionsAsync(connection, userId, cancellationToken);
        var sessionById = sessions.ToDictionary(item => item.Id, StringComparer.Ordinal);

        foreach (var session in sessions)
        {
            if (routines.TryGetValue(session.RoutineId, out var routine))
            {
                routine.Sessions.Add(session);
            }
        }

        var exercises = await LoadExercisesAsync(connection, userId, cancellationToken);
        var exerciseById = exercises.ToDictionary(item => item.Id, StringComparer.Ordinal);

        foreach (var exercise in exercises)
        {
            if (sessionById.TryGetValue(exercise.SessionId, out var session))
            {
                session.Exercises.Add(exercise);
            }
        }

        var sets = await LoadPlannedSetsAsync(connection, userId, cancellationToken);
        foreach (var setItem in sets)
        {
            if (exerciseById.TryGetValue(setItem.SessionExerciseId, out var exercise))
            {
                exercise.PlannedSets.Add(setItem);
            }
        }

        return routines.Values.ToArray();
    }

    public async Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        var routines = await ListByUserAsync(userId, cancellationToken);
        return routines.FirstOrDefault(item => item.Id == routineId);
    }

    public async Task UpsertAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        await SqlRoutinesSchemaInitializer.EnsureAsync(_connectionString, cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string routineSql = """
            MERGE RoutinesV2 AS target
            USING (SELECT @RoutineId AS RoutineId) AS source
            ON target.RoutineId = source.RoutineId
            WHEN MATCHED THEN
                UPDATE SET
                    UserId = @UserId,
                    Title = @Title,
                    Goal = @Goal,
                    IsDeleted = @IsDeleted,
                    UpdatedAt = @UpdatedAt,
                    DeletedAt = @DeletedAt
            WHEN NOT MATCHED THEN
                INSERT (RoutineId, UserId, Title, Goal, IsDeleted, CreatedAt, UpdatedAt, DeletedAt)
                VALUES (@RoutineId, @UserId, @Title, @Goal, @IsDeleted, @CreatedAt, @UpdatedAt, @DeletedAt);
            """;

        await using (var command = new SqlCommand(routineSql, connection, (SqlTransaction)transaction))
        {
            command.Parameters.AddWithValue("@RoutineId", routine.Id);
            command.Parameters.AddWithValue("@UserId", routine.UserId);
            command.Parameters.AddWithValue("@Title", routine.Title);
            command.Parameters.AddWithValue("@Goal", (object?)routine.Goal ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsDeleted", routine.IsDeleted);
            command.Parameters.AddWithValue("@CreatedAt", routine.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", routine.UpdatedAt);
            command.Parameters.AddWithValue("@DeletedAt", (object?)routine.DeletedAt ?? DBNull.Value);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        const string deleteChildrenSql = """
            DELETE plannedSet
            FROM PlannedSetsV2 plannedSet
            INNER JOIN SessionExercisesV2 exercise ON exercise.SessionExerciseId = plannedSet.SessionExerciseId
            WHERE exercise.RoutineId = @RoutineId;

            DELETE FROM SessionExercisesV2 WHERE RoutineId = @RoutineId;
            DELETE FROM RoutineSessionsV2 WHERE RoutineId = @RoutineId;
            """;

        await using (var command = new SqlCommand(deleteChildrenSql, connection, (SqlTransaction)transaction))
        {
            command.Parameters.AddWithValue("@RoutineId", routine.Id);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var session in routine.Sessions)
        {
            const string insertSessionSql = """
                INSERT INTO RoutineSessionsV2 (SessionId, RoutineId, Name, DaysOfWeek, IsDeleted, CreatedAt, UpdatedAt)
                VALUES (@SessionId, @RoutineId, @Name, @DaysOfWeek, @IsDeleted, @CreatedAt, @UpdatedAt);
                """;

            await using (var command = new SqlCommand(insertSessionSql, connection, (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@SessionId", session.Id);
                command.Parameters.AddWithValue("@RoutineId", routine.Id);
                command.Parameters.AddWithValue("@Name", session.Name);
                command.Parameters.AddWithValue("@DaysOfWeek", SerializeDays(session.DaysOfWeek));
                command.Parameters.AddWithValue("@IsDeleted", session.IsDeleted);
                command.Parameters.AddWithValue("@CreatedAt", session.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", session.UpdatedAt);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var exercise in session.Exercises)
            {
                const string insertExerciseSql = """
                    INSERT INTO SessionExercisesV2 (SessionExerciseId, RoutineId, SessionId, ExerciseId, Name, IsDeleted, CreatedAt, UpdatedAt)
                    VALUES (@SessionExerciseId, @RoutineId, @SessionId, @ExerciseId, @Name, @IsDeleted, @CreatedAt, @UpdatedAt);
                    """;

                await using (var command = new SqlCommand(insertExerciseSql, connection, (SqlTransaction)transaction))
                {
                    command.Parameters.AddWithValue("@SessionExerciseId", exercise.Id);
                    command.Parameters.AddWithValue("@RoutineId", routine.Id);
                    command.Parameters.AddWithValue("@SessionId", session.Id);
                    command.Parameters.AddWithValue("@ExerciseId", exercise.ExerciseId);
                    command.Parameters.AddWithValue("@Name", exercise.Name);
                    command.Parameters.AddWithValue("@IsDeleted", exercise.IsDeleted);
                    command.Parameters.AddWithValue("@CreatedAt", exercise.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", exercise.UpdatedAt);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                foreach (var setItem in exercise.PlannedSets)
                {
                    const string insertSetSql = """
                        INSERT INTO PlannedSetsV2 (PlannedSetId, SessionExerciseId, Repetitions, WeightKg, [Order], CreatedAt, UpdatedAt)
                        VALUES (@PlannedSetId, @SessionExerciseId, @Repetitions, @WeightKg, @Order, @CreatedAt, @UpdatedAt);
                        """;

                    await using var command = new SqlCommand(insertSetSql, connection, (SqlTransaction)transaction);
                    command.Parameters.AddWithValue("@PlannedSetId", setItem.Id);
                    command.Parameters.AddWithValue("@SessionExerciseId", exercise.Id);
                    command.Parameters.AddWithValue("@Repetitions", setItem.Repetitions);
                    command.Parameters.AddWithValue("@WeightKg", setItem.WeightKg);
                    command.Parameters.AddWithValue("@Order", setItem.Order);
                    command.Parameters.AddWithValue("@CreatedAt", setItem.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", setItem.UpdatedAt);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<List<RoutineSession>> LoadSessionsAsync(SqlConnection connection, string userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT session.SessionId, session.RoutineId, session.Name, session.DaysOfWeek, session.IsDeleted, session.CreatedAt
            FROM RoutineSessionsV2 session
            INNER JOIN RoutinesV2 routine ON routine.RoutineId = session.RoutineId
            WHERE routine.UserId = @UserId;
            """;

        var result = new List<RoutineSession>();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var session = new RoutineSession
            {
                Id = reader.GetString(0),
                RoutineId = reader.GetString(1),
                Name = reader.GetString(2),
                DaysOfWeek = DeserializeDays(reader.GetString(3)),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(5),
            };

            if (reader.GetBoolean(4))
            {
                session.SoftDelete();
            }

            result.Add(session);
        }

        return result;
    }

    private static async Task<List<SessionExerciseLink>> LoadExercisesAsync(SqlConnection connection, string userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT exercise.SessionExerciseId, exercise.RoutineId, exercise.SessionId, exercise.ExerciseId, exercise.Name, exercise.IsDeleted, exercise.CreatedAt
            FROM SessionExercisesV2 exercise
            INNER JOIN RoutinesV2 routine ON routine.RoutineId = exercise.RoutineId
            WHERE routine.UserId = @UserId;
            """;

        var result = new List<SessionExerciseLink>();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var exercise = new SessionExerciseLink
            {
                Id = reader.GetString(0),
                RoutineId = reader.GetString(1),
                SessionId = reader.GetString(2),
                ExerciseId = reader.GetString(3),
                Name = reader.GetString(4),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(6),
            };

            if (reader.GetBoolean(5))
            {
                exercise.Unlink();
            }

            result.Add(exercise);
        }

        return result;
    }

    private static async Task<List<PlannedSet>> LoadPlannedSetsAsync(SqlConnection connection, string userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT plannedSet.PlannedSetId, plannedSet.SessionExerciseId, plannedSet.Repetitions, plannedSet.WeightKg, plannedSet.[Order], plannedSet.CreatedAt
            FROM PlannedSetsV2 plannedSet
            INNER JOIN SessionExercisesV2 exercise ON exercise.SessionExerciseId = plannedSet.SessionExerciseId
            INNER JOIN RoutinesV2 routine ON routine.RoutineId = exercise.RoutineId
            WHERE routine.UserId = @UserId;
            """;

        var result = new List<PlannedSet>();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var setItem = new PlannedSet(
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDecimal(3),
                reader.GetInt32(4))
            {
                Id = reader.GetString(0),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(5),
            };

            result.Add(setItem);
        }

        return result;
    }

    private static string SerializeDays(IEnumerable<string> daysOfWeek)
    {
        return string.Join(',', daysOfWeek.Select(day => day.Trim().ToLowerInvariant()).Distinct(StringComparer.Ordinal));
    }

    private static List<string> DeserializeDays(string storedValue)
    {
        return storedValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
