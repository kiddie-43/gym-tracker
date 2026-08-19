using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations;

public partial class AddUserIdToRoutines : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
IF COL_LENGTH('Routines', 'UserId') IS NULL
BEGIN
    ALTER TABLE [Routines]
    ADD [UserId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Routines_UserId] DEFAULT '11111111-1111-1111-1111-111111111111';
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Routines_UserId'
      AND object_id = OBJECT_ID(N'[Routines]')
)
BEGIN
    CREATE INDEX [IX_Routines_UserId]
    ON [Routines]([UserId]);
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Routines_UserId'
      AND object_id = OBJECT_ID(N'[Routines]')
)
BEGIN
    DROP INDEX [IX_Routines_UserId] ON [Routines];
END

IF COL_LENGTH('Routines', 'UserId') IS NOT NULL
BEGIN
    ALTER TABLE [Routines] DROP CONSTRAINT [DF_Routines_UserId];
    ALTER TABLE [Routines] DROP COLUMN [UserId];
END
""");
    }
}