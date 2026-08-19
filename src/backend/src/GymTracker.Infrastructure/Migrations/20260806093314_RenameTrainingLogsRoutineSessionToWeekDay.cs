using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTrainingLogsRoutineSessionToWeekDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_RoutineId",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_SessionId",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_SessionId_ExerciseCode",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "RoutineId",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "TrainingLogs");

            migrationBuilder.AddColumn<int>(
                name: "DayNumber",
                table: "TrainingLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeekNumber",
                table: "TrainingLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_DayNumber",
                table: "TrainingLogs",
                column: "DayNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_WeekNumber",
                table: "TrainingLogs",
                column: "WeekNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_WeekNumber_DayNumber_ExerciseCode",
                table: "TrainingLogs",
                columns: new[] { "WeekNumber", "DayNumber", "ExerciseCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_DayNumber",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_WeekNumber",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_WeekNumber_DayNumber_ExerciseCode",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "DayNumber",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "WeekNumber",
                table: "TrainingLogs");

            migrationBuilder.AddColumn<string>(
                name: "RoutineId",
                table: "TrainingLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "TrainingLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_RoutineId",
                table: "TrainingLogs",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_SessionId",
                table: "TrainingLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_SessionId_ExerciseCode",
                table: "TrainingLogs",
                columns: new[] { "SessionId", "ExerciseCode" });
        }
    }
}
