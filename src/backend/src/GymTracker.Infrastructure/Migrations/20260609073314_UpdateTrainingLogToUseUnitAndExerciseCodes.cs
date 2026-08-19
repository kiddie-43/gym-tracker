using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrainingLogToUseUnitAndExerciseCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingLogs_Units_MetricId",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_ExerciseId",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_MetricId",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_SessionId_ExerciseId",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "MetricId",
                table: "TrainingLogs");

            migrationBuilder.AddColumn<string>(
                name: "ExerciseCode",
                table: "TrainingLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitCode",
                table: "TrainingLogs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_ExerciseCode",
                table: "TrainingLogs",
                column: "ExerciseCode");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_SessionId_ExerciseCode",
                table: "TrainingLogs",
                columns: new[] { "SessionId", "ExerciseCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_UnitCode",
                table: "TrainingLogs",
                column: "UnitCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_ExerciseCode",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_SessionId_ExerciseCode",
                table: "TrainingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrainingLogs_UnitCode",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "ExerciseCode",
                table: "TrainingLogs");

            migrationBuilder.DropColumn(
                name: "UnitCode",
                table: "TrainingLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseId",
                table: "TrainingLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MetricId",
                table: "TrainingLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_ExerciseId",
                table: "TrainingLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_MetricId",
                table: "TrainingLogs",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLogs_SessionId_ExerciseId",
                table: "TrainingLogs",
                columns: new[] { "SessionId", "ExerciseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingLogs_Units_MetricId",
                table: "TrainingLogs",
                column: "MetricId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
