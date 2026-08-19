using GymTracker.Infrastructure.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.Infrastructure;

public static class DatabaseMigrationExtensions
{
    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        var sqlConnectionString = app.Configuration
            .GetSection("Sql")
            .GetValue<string>("ConnectionString");

        if (string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();

        db.Database.Migrate();

        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'MeasurementTypeId') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MeasurementTypeId];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'MeasurementTypeCode') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MeasurementTypeCode];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'MeasurementTypeIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MeasurementTypeIds];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'MeasurementTypeNames') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MeasurementTypeNames];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'PrimaryMuscleId') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [PrimaryMuscleId];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'PrimaryMuscleCode') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [PrimaryMuscleCode];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'PrimaryMuscleIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [PrimaryMuscleIds];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'SecondaryMuscleId') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [SecondaryMuscleId];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'SecondaryMuscleCode') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [SecondaryMuscleCode];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'SecondaryMuscleIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [SecondaryMuscleIds];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'MuscleGroupId') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MuscleGroupId];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'MuscleGroupIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MuscleGroupIds];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'ExerciseTypeId') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [ExerciseTypeId];");
        db.Database.ExecuteSqlRaw("IF COL_LENGTH('Exercises', 'FormTypeId') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [FormTypeId];");

        db.Database.ExecuteSqlRaw("""
IF OBJECT_ID(N'[Exercices]', N'U') IS NULL
BEGIN
    CREATE TABLE [Exercices]
    (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ExerciseType] nvarchar(30) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Exercices] PRIMARY KEY ([Id])
    );
END

IF COL_LENGTH('Exercices', 'ExerciseType') IS NULL
BEGIN
    ALTER TABLE [Exercices]
    ADD [ExerciseType] nvarchar(30) NOT NULL
        CONSTRAINT [DF_Exercices_ExerciseType] DEFAULT N'STRENGTH';
END

IF COL_LENGTH('Exercices', 'ExerciseType') IS NOT NULL
BEGIN
    EXEC(N'UPDATE [Exercices]
SET [ExerciseType] = N''STRENGTH''
WHERE [ExerciseType] IS NULL;');
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Exercices_Code_Active'
      AND object_id = OBJECT_ID(N'[Exercices]')
)
BEGIN
    CREATE UNIQUE INDEX [IX_Exercices_Code_Active]
    ON [Exercices]([Code])
    WHERE [DeletedAt] IS NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Exercices_DeletedAt'
      AND object_id = OBJECT_ID(N'[Exercices]')
)
AND COL_LENGTH('Exercices', 'DeletedAt') IS NOT NULL
BEGIN
    CREATE INDEX [IX_Exercices_DeletedAt]
    ON [Exercices]([DeletedAt]);
END

IF OBJECT_ID(N'[ExerciceUnits]', N'U') IS NULL
BEGIN
    CREATE TABLE [ExerciceUnits]
    (
        [ExerciceId] uniqueidentifier NOT NULL,
        [UnitId] uniqueidentifier NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ExerciceUnits] PRIMARY KEY ([ExerciceId], [UnitId]),
        CONSTRAINT [FK_ExerciceUnits_Exercices_ExerciceId] FOREIGN KEY ([ExerciceId]) REFERENCES [Exercices]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExerciceUnits_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units]([Id]) ON DELETE NO ACTION
    );
END

IF COL_LENGTH('ExerciceUnits', 'Id') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [Id] uniqueidentifier NOT NULL
        CONSTRAINT [DF_ExerciceUnits_Id] DEFAULT NEWID();
END

IF COL_LENGTH('ExerciceUnits', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [CreatedAt] datetimeoffset NOT NULL
        CONSTRAINT [DF_ExerciceUnits_CreatedAt] DEFAULT SYSDATETIMEOFFSET();
END

IF COL_LENGTH('ExerciceUnits', 'CreatedBy') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [CreatedBy] uniqueidentifier NOT NULL
        CONSTRAINT [DF_ExerciceUnits_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000';
END

IF COL_LENGTH('ExerciceUnits', 'UpdatedAt') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [UpdatedAt] datetimeoffset NULL;
END

IF COL_LENGTH('ExerciceUnits', 'UpdatedBy') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [UpdatedBy] uniqueidentifier NULL;
END

IF COL_LENGTH('ExerciceUnits', 'DeletedAt') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [DeletedAt] datetimeoffset NULL;
END

IF COL_LENGTH('ExerciceUnits', 'DeletedBy') IS NULL
BEGIN
    ALTER TABLE [ExerciceUnits]
    ADD [DeletedBy] uniqueidentifier NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ExerciceUnits_DeletedAt'
      AND object_id = OBJECT_ID(N'[ExerciceUnits]')
)
AND COL_LENGTH('ExerciceUnits', 'DeletedAt') IS NOT NULL
BEGIN
    CREATE INDEX [IX_ExerciceUnits_DeletedAt]
    ON [ExerciceUnits]([DeletedAt]);
END

IF OBJECT_ID(N'[ExerciceMuscles]', N'U') IS NULL
BEGIN
    CREATE TABLE [ExerciceMuscles]
    (
        [ExerciceId] uniqueidentifier NOT NULL,
        [MuscleId] uniqueidentifier NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ExerciceMuscles] PRIMARY KEY ([ExerciceId], [MuscleId], [Type]),
        CONSTRAINT [FK_ExerciceMuscles_Exercices_ExerciceId] FOREIGN KEY ([ExerciceId]) REFERENCES [Exercices]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExerciceMuscles_Muscles_MuscleId] FOREIGN KEY ([MuscleId]) REFERENCES [Muscles]([Id]) ON DELETE NO ACTION
    );
END

IF COL_LENGTH('ExerciceMuscles', 'Id') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [Id] uniqueidentifier NOT NULL
        CONSTRAINT [DF_ExerciceMuscles_Id] DEFAULT NEWID();
END

IF COL_LENGTH('ExerciceMuscles', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [CreatedAt] datetimeoffset NOT NULL
        CONSTRAINT [DF_ExerciceMuscles_CreatedAt] DEFAULT SYSDATETIMEOFFSET();
END

IF COL_LENGTH('ExerciceMuscles', 'CreatedBy') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [CreatedBy] uniqueidentifier NOT NULL
        CONSTRAINT [DF_ExerciceMuscles_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000';
END

IF COL_LENGTH('ExerciceMuscles', 'UpdatedAt') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [UpdatedAt] datetimeoffset NULL;
END

IF COL_LENGTH('ExerciceMuscles', 'UpdatedBy') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [UpdatedBy] uniqueidentifier NULL;
END

IF COL_LENGTH('ExerciceMuscles', 'DeletedAt') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [DeletedAt] datetimeoffset NULL;
END

IF COL_LENGTH('ExerciceMuscles', 'DeletedBy') IS NULL
BEGIN
    ALTER TABLE [ExerciceMuscles]
    ADD [DeletedBy] uniqueidentifier NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ExerciceMuscles_DeletedAt'
      AND object_id = OBJECT_ID(N'[ExerciceMuscles]')
)
AND COL_LENGTH('ExerciceMuscles', 'DeletedAt') IS NOT NULL
BEGIN
    CREATE INDEX [IX_ExerciceMuscles_DeletedAt]
    ON [ExerciceMuscles]([DeletedAt]);
END
""");

        db.Database.ExecuteSqlRaw("""
IF OBJECT_ID(N'[MonthlyPlans]', N'U') IS NULL
BEGIN
    CREATE TABLE [MonthlyPlans]
    (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [ActiveDays] int NOT NULL,
        [MigrationVersion] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_MonthlyPlans] PRIMARY KEY ([Id])
    );
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_MonthlyPlans_UserId_Active'
      AND object_id = OBJECT_ID(N'[MonthlyPlans]')
)
BEGIN
    CREATE UNIQUE INDEX [IX_MonthlyPlans_UserId_Active]
    ON [MonthlyPlans]([UserId])
    WHERE [DeletedAt] IS NULL;
END

IF OBJECT_ID(N'[PlanWeeks]', N'U') IS NULL
BEGIN
    CREATE TABLE [PlanWeeks]
    (
        [MonthlyPlanId] uniqueidentifier NOT NULL,
        [WeekNumber] int NOT NULL,
        CONSTRAINT [PK_PlanWeeks] PRIMARY KEY ([MonthlyPlanId], [WeekNumber]),
        CONSTRAINT [FK_PlanWeeks_MonthlyPlans_MonthlyPlanId]
            FOREIGN KEY ([MonthlyPlanId]) REFERENCES [MonthlyPlans]([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[PlanDays]', N'U') IS NULL
BEGIN
    CREATE TABLE [PlanDays]
    (
        [MonthlyPlanId] uniqueidentifier NOT NULL,
        [WeekNumber] int NOT NULL,
        [DayNumber] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [TruncatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_PlanDays] PRIMARY KEY ([MonthlyPlanId], [WeekNumber], [DayNumber]),
        CONSTRAINT [FK_PlanDays_PlanWeeks_MonthlyPlanId_WeekNumber]
            FOREIGN KEY ([MonthlyPlanId], [WeekNumber]) REFERENCES [PlanWeeks]([MonthlyPlanId], [WeekNumber]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NULL
BEGIN
    CREATE TABLE [PlannedExercises]
    (
        [Id] uniqueidentifier NOT NULL,
        [MonthlyPlanId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [WeekNumber] int NOT NULL,
        [DayNumber] int NOT NULL,
        [ExerciseId] uniqueidentifier NOT NULL,
        [OrderIndex] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PlannedExercises] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlannedExercises_MonthlyPlans_MonthlyPlanId]
            FOREIGN KEY ([MonthlyPlanId]) REFERENCES [MonthlyPlans]([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'Notes') IS NOT NULL
BEGIN
    ALTER TABLE [PlannedExercises] DROP COLUMN [Notes];
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'UserId') IS NULL
BEGIN
    ALTER TABLE [PlannedExercises]
    ADD [UserId] uniqueidentifier NULL;

    EXEC(N'UPDATE pe
SET pe.[UserId] = mp.[UserId]
FROM [PlannedExercises] pe
JOIN [MonthlyPlans] mp ON mp.[Id] = pe.[MonthlyPlanId]
WHERE pe.[UserId] IS NULL;');

    EXEC(N'DELETE FROM [PlannedExercises] WHERE [UserId] IS NULL;');

    EXEC(N'ALTER TABLE [PlannedExercises] ALTER COLUMN [UserId] uniqueidentifier NOT NULL;');
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'UserId') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_PlannedExercises_UserId'
          AND object_id = OBJECT_ID(N'[PlannedExercises]')
   )
BEGIN
    CREATE INDEX [IX_PlannedExercises_UserId]
    ON [PlannedExercises]([UserId]);
END

IF COL_LENGTH('PlannedExercises', 'ExerciseId') IS NULL
   AND COL_LENGTH('PlannedExercises', 'ExerciceId') IS NOT NULL
BEGIN
    EXEC sp_rename 'PlannedExercises.ExerciceId', 'ExerciseId', 'COLUMN';
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'ExerciseId') IS NOT NULL
BEGIN
    DELETE pe
    FROM [PlannedExercises] pe
    LEFT JOIN [Exercices] e ON e.[Id] = pe.[ExerciseId]
    WHERE e.[Id] IS NULL;
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'ExerciseId') IS NOT NULL
   AND OBJECT_ID(N'[Exercices]', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_PlannedExercises_Exercices_ExerciseId'
          AND parent_object_id = OBJECT_ID(N'[PlannedExercises]')
   )
BEGIN
    ALTER TABLE [PlannedExercises]
    ADD CONSTRAINT [FK_PlannedExercises_Exercices_ExerciseId]
        FOREIGN KEY ([ExerciseId]) REFERENCES [Exercices]([Id]) ON DELETE NO ACTION;
END

IF OBJECT_ID(N'[PlannedExercises]', N'U') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'MonthlyPlanId') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'WeekNumber') IS NOT NULL
   AND COL_LENGTH('PlannedExercises', 'DayNumber') IS NOT NULL
   AND OBJECT_ID(N'[PlanDays]', N'U') IS NOT NULL
BEGIN
    DELETE pe
    FROM [PlannedExercises] pe
    LEFT JOIN [PlanDays] pd
      ON pd.[MonthlyPlanId] = pe.[MonthlyPlanId]
     AND pd.[WeekNumber] = pe.[WeekNumber]
     AND pd.[DayNumber] = pe.[DayNumber]
    WHERE pd.[MonthlyPlanId] IS NULL;

    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_PlannedExercises_PlanDays_MonthlyPlanId_WeekNumber_DayNumber'
          AND parent_object_id = OBJECT_ID(N'[PlannedExercises]')
          AND delete_referential_action_desc <> N'NO_ACTION'
    )
    BEGIN
        ALTER TABLE [PlannedExercises]
        DROP CONSTRAINT [FK_PlannedExercises_PlanDays_MonthlyPlanId_WeekNumber_DayNumber];
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_PlannedExercises_PlanDays_MonthlyPlanId_WeekNumber_DayNumber'
          AND parent_object_id = OBJECT_ID(N'[PlannedExercises]')
    )
    BEGIN
        ALTER TABLE [PlannedExercises]
        ADD CONSTRAINT [FK_PlannedExercises_PlanDays_MonthlyPlanId_WeekNumber_DayNumber]
            FOREIGN KEY ([MonthlyPlanId], [WeekNumber], [DayNumber])
            REFERENCES [PlanDays]([MonthlyPlanId], [WeekNumber], [DayNumber])
            ON DELETE NO ACTION;
    END
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_PlannedExercises_MonthlyPlan_Week_Day_Order'
      AND object_id = OBJECT_ID(N'[PlannedExercises]')
)
BEGIN
    CREATE INDEX [IX_PlannedExercises_MonthlyPlan_Week_Day_Order]
    ON [PlannedExercises]([MonthlyPlanId], [WeekNumber], [DayNumber], [OrderIndex]);
END

IF OBJECT_ID(N'[HistoricalExerciseRecords]', N'U') IS NULL
BEGIN
    CREATE TABLE [HistoricalExerciseRecords]
    (
        [Id] uniqueidentifier NOT NULL,
        [SourcePlannedExerciseId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [WeekNumber] int NOT NULL,
        [DayNumber] int NOT NULL,
        [Payload] nvarchar(4000) NOT NULL,
        [RecordedAt] datetimeoffset NOT NULL,
        [SourceReason] nvarchar(100) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_HistoricalExerciseRecords] PRIMARY KEY ([Id])
    );
END
""");

        return app;
    }
}