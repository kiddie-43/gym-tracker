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
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Exercices] PRIMARY KEY ([Id])
    );
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

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ExerciceUnits_DeletedAt'
      AND object_id = OBJECT_ID(N'[ExerciceUnits]')
)
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

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ExerciceMuscles_DeletedAt'
      AND object_id = OBJECT_ID(N'[ExerciceMuscles]')
)
BEGIN
    CREATE INDEX [IX_ExerciceMuscles_DeletedAt]
    ON [ExerciceMuscles]([DeletedAt]);
END
""");

        return app;
    }
}