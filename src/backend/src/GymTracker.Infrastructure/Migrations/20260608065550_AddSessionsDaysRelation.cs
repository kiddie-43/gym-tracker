using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionsDaysRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionsDays",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionsDays", x => new { x.SessionId, x.DayOfWeek });
                    table.ForeignKey(
                        name: "FK_SessionsDays_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionsDays_DayOfWeek",
                table: "SessionsDays",
                column: "DayOfWeek");

            migrationBuilder.Sql(
                """
IF COL_LENGTH('Sessions', 'DayOfWeek') IS NOT NULL
BEGIN
    INSERT INTO [SessionsDays] ([SessionId], [DayOfWeek])
    SELECT [Id], [DayOfWeek]
    FROM [Sessions]
    WHERE [DayOfWeek] IS NOT NULL AND LTRIM(RTRIM([DayOfWeek])) <> '';
END
""");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "Sessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionsDays");

            migrationBuilder.AddColumn<string>(
                name: "DayOfWeek",
                table: "Sessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
