using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutinesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF OBJECT_ID(N'[Routines]', N'U') IS NULL
BEGIN
    CREATE TABLE [Routines]
    (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Routines] PRIMARY KEY ([Id])
    );
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Routines_DeletedAt'
      AND object_id = OBJECT_ID(N'[Routines]')
)
BEGIN
    CREATE INDEX [IX_Routines_DeletedAt]
    ON [Routines]([DeletedAt]);
END
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF OBJECT_ID(N'[Routines]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [Routines];
END
""");
        }
    }
}
