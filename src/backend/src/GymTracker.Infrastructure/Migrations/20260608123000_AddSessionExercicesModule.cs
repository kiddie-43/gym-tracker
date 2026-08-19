using System;
using GymTracker.Infrastructure.Admin;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations
{
    [DbContext(typeof(AdminDbContext))]
    [Migration("20260608123000_AddSessionExercicesModule")]
    public partial class AddSessionExercicesModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionExercices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionExercices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionExercices_Exercices_ExerciceId",
                        column: x => x.ExerciceId,
                        principalTable: "Exercices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionExercices_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercices_DeletedAt",
                table: "SessionExercices",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercices_ExerciceId",
                table: "SessionExercices",
                column: "ExerciceId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercices_SessionId",
                table: "SessionExercices",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercices_SessionId_ExerciceId",
                table: "SessionExercices",
                columns: new[] { "SessionId", "ExerciceId" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionExercices");
        }
    }
}
