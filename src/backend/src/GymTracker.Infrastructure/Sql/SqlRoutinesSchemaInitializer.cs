using System.Threading;

using Microsoft.Data.SqlClient;

namespace GymTracker.Infrastructure.Sql;

internal static class SqlRoutinesSchemaInitializer
{
    private static int _initialized;
    private static readonly SemaphoreSlim SchemaLock = new(1, 1);

    public static async Task EnsureAsync(string connectionString, CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref _initialized) == 1)
        {
            return;
        }

        await SchemaLock.WaitAsync(cancellationToken);
        try
        {
            if (Volatile.Read(ref _initialized) == 1)
            {
                return;
            }

            const string sql = """
                IF OBJECT_ID(N'RoutinesV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE RoutinesV2 (
                        RoutineId NVARCHAR(32) NOT NULL,
                        UserId NVARCHAR(128) NOT NULL,
                        Title NVARCHAR(200) NOT NULL,
                        Goal NVARCHAR(1000) NULL,
                        IsDeleted BIT NOT NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        DeletedAt DATETIMEOFFSET NULL,
                        CONSTRAINT PK_RoutinesV2 PRIMARY KEY (RoutineId)
                    );
                END;

                IF OBJECT_ID(N'RoutineSessionsV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE RoutineSessionsV2 (
                        SessionId NVARCHAR(32) NOT NULL,
                        RoutineId NVARCHAR(32) NOT NULL,
                        Name NVARCHAR(200) NOT NULL,
                        DaysOfWeek NVARCHAR(100) NOT NULL,
                        IsDeleted BIT NOT NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_RoutineSessionsV2 PRIMARY KEY (SessionId),
                        CONSTRAINT FK_RoutineSessionsV2_RoutinesV2 FOREIGN KEY (RoutineId) REFERENCES RoutinesV2(RoutineId) ON DELETE CASCADE
                    );
                END;

                IF OBJECT_ID(N'SessionExercisesV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE SessionExercisesV2 (
                        SessionExerciseId NVARCHAR(32) NOT NULL,
                        RoutineId NVARCHAR(32) NOT NULL,
                        SessionId NVARCHAR(32) NOT NULL,
                        ExerciseId NVARCHAR(128) NOT NULL,
                        Name NVARCHAR(250) NOT NULL,
                        IsDeleted BIT NOT NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_SessionExercisesV2 PRIMARY KEY (SessionExerciseId),
                        CONSTRAINT FK_SessionExercisesV2_RoutineSessionsV2 FOREIGN KEY (SessionId) REFERENCES RoutineSessionsV2(SessionId) ON DELETE CASCADE,
                        CONSTRAINT FK_SessionExercisesV2_RoutinesV2 FOREIGN KEY (RoutineId) REFERENCES RoutinesV2(RoutineId)
                    );
                END;

                IF OBJECT_ID(N'PlannedSetsV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE PlannedSetsV2 (
                        PlannedSetId NVARCHAR(32) NOT NULL,
                        SessionExerciseId NVARCHAR(32) NOT NULL,
                        Repetitions INT NOT NULL,
                        WeightKg DECIMAL(10,2) NOT NULL,
                        [Order] INT NOT NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_PlannedSetsV2 PRIMARY KEY (PlannedSetId),
                        CONSTRAINT FK_PlannedSetsV2_SessionExercisesV2 FOREIGN KEY (SessionExerciseId) REFERENCES SessionExercisesV2(SessionExerciseId) ON DELETE CASCADE
                    );
                END;

                IF OBJECT_ID(N'TrainingFlowStatesV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE TrainingFlowStatesV2 (
                        UserId NVARCHAR(128) NOT NULL,
                        IsLocked BIT NOT NULL,
                        RoutineId NVARCHAR(32) NOT NULL,
                        SessionId NVARCHAR(32) NULL,
                        ExerciseId NVARCHAR(128) NULL,
                        StepNode NVARCHAR(50) NOT NULL,
                        LastUpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_TrainingFlowStatesV2 PRIMARY KEY (UserId)
                    );
                END;

                IF OBJECT_ID(N'ExerciseTrainingLogsV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExerciseTrainingLogsV2 (
                        LogId NVARCHAR(32) NOT NULL,
                        UserId NVARCHAR(128) NOT NULL,
                        RoutineId NVARCHAR(32) NOT NULL,
                        SessionId NVARCHAR(32) NOT NULL,
                        ExerciseId NVARCHAR(128) NOT NULL,
                        Notes NVARCHAR(MAX) NULL,
                        CreatedAt DATETIMEOFFSET NOT NULL,
                        UpdatedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_ExerciseTrainingLogsV2 PRIMARY KEY (LogId)
                    );
                END;

                IF OBJECT_ID(N'ExerciseTrainingLogSetsV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExerciseTrainingLogSetsV2 (
                        LogId NVARCHAR(32) NOT NULL,
                        [Order] INT NOT NULL,
                        Repetitions INT NOT NULL,
                        WeightKg DECIMAL(10,2) NOT NULL,
                        CONSTRAINT PK_ExerciseTrainingLogSetsV2 PRIMARY KEY (LogId, [Order]),
                        CONSTRAINT FK_ExerciseTrainingLogSetsV2_Logs FOREIGN KEY (LogId) REFERENCES ExerciseTrainingLogsV2(LogId) ON DELETE CASCADE
                    );
                END;

                IF OBJECT_ID(N'ExerciseTrainingLogAttachmentsV2', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExerciseTrainingLogAttachmentsV2 (
                        AttachmentId NVARCHAR(32) NOT NULL,
                        LogId NVARCHAR(32) NOT NULL,
                        AttachmentType NVARCHAR(40) NOT NULL,
                        Url NVARCHAR(1200) NOT NULL,
                        UploadedAt DATETIMEOFFSET NOT NULL,
                        CONSTRAINT PK_ExerciseTrainingLogAttachmentsV2 PRIMARY KEY (AttachmentId),
                        CONSTRAINT FK_ExerciseTrainingLogAttachmentsV2_Logs FOREIGN KEY (LogId) REFERENCES ExerciseTrainingLogsV2(LogId) ON DELETE CASCADE
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RoutinesV2_UserId' AND object_id = OBJECT_ID(N'RoutinesV2'))
                BEGIN
                    CREATE INDEX IX_RoutinesV2_UserId ON RoutinesV2(UserId);
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RoutineSessionsV2_RoutineId' AND object_id = OBJECT_ID(N'RoutineSessionsV2'))
                BEGIN
                    CREATE INDEX IX_RoutineSessionsV2_RoutineId ON RoutineSessionsV2(RoutineId);
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SessionExercisesV2_RoutineId' AND object_id = OBJECT_ID(N'SessionExercisesV2'))
                BEGIN
                    CREATE INDEX IX_SessionExercisesV2_RoutineId ON SessionExercisesV2(RoutineId);
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExerciseTrainingLogsV2_UserExercise' AND object_id = OBJECT_ID(N'ExerciseTrainingLogsV2'))
                BEGIN
                    CREATE INDEX IX_ExerciseTrainingLogsV2_UserExercise ON ExerciseTrainingLogsV2(UserId, ExerciseId, CreatedAt DESC);
                END;
                """;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            Volatile.Write(ref _initialized, 1);
        }
        finally
        {
            SchemaLock.Release();
        }
    }
}
