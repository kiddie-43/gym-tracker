using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations;

public partial class AddSessionsModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
IF OBJECT_ID(N'[Sessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sessions]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [DayOfWeek] NVARCHAR(20) NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [RoutineId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt] DATETIMEOFFSET NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAt] DATETIMEOFFSET NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL,
        [DeletedAt] DATETIMEOFFSET NULL,
        [DeletedBy] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_Sessions_DeletedAt] ON [Sessions]([DeletedAt]);
    CREATE INDEX [IX_Sessions_RoutineId] ON [Sessions]([RoutineId]);
    CREATE INDEX [IX_Sessions_UserId] ON [Sessions]([UserId]);
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
IF OBJECT_ID(N'[Sessions]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [Sessions];
END
""");
    }
}