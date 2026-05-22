using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseRelationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('Exercises', 'MeasurementTypeIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [MeasurementTypeIds];");
            migrationBuilder.Sql("IF COL_LENGTH('Exercises', 'PrimaryMuscleIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [PrimaryMuscleIds];");
            migrationBuilder.Sql("IF COL_LENGTH('Exercises', 'SecondaryMuscleIds') IS NOT NULL ALTER TABLE [Exercises] DROP COLUMN [SecondaryMuscleIds];");

            migrationBuilder.CreateTable(
                name: "ExerciseMeasurementTypes",
                columns: table => new
                {
                    ExerciseId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MeasurementTypeId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMeasurementTypes", x => new { x.ExerciseId, x.MeasurementTypeId });
                    table.ForeignKey(
                        name: "FK_ExerciseMeasurementTypes_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMeasurementTypes_MeasurementTypes_MeasurementTypeId",
                        column: x => x.MeasurementTypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExercisePrimaryMuscles",
                columns: table => new
                {
                    ExerciseId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MuscleId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExercisePrimaryMuscles", x => new { x.ExerciseId, x.MuscleId });
                    table.ForeignKey(
                        name: "FK_ExercisePrimaryMuscles_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExercisePrimaryMuscles_Muscles_MuscleId",
                        column: x => x.MuscleId,
                        principalTable: "Muscles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSecondaryMuscles",
                columns: table => new
                {
                    ExerciseId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MuscleId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSecondaryMuscles", x => new { x.ExerciseId, x.MuscleId });
                    table.ForeignKey(
                        name: "FK_ExerciseSecondaryMuscles_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSecondaryMuscles_Muscles_MuscleId",
                        column: x => x.MuscleId,
                        principalTable: "Muscles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMeasurementTypes_MeasurementTypeId",
                table: "ExerciseMeasurementTypes",
                column: "MeasurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExercisePrimaryMuscles_MuscleId",
                table: "ExercisePrimaryMuscles",
                column: "MuscleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSecondaryMuscles_MuscleId",
                table: "ExerciseSecondaryMuscles",
                column: "MuscleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseMeasurementTypes");

            migrationBuilder.DropTable(
                name: "ExercisePrimaryMuscles");

            migrationBuilder.DropTable(
                name: "ExerciseSecondaryMuscles");

            migrationBuilder.Sql("IF COL_LENGTH('Exercises', 'MeasurementTypeIds') IS NULL ALTER TABLE [Exercises] ADD [MeasurementTypeIds] nvarchar(max) NOT NULL DEFAULT N'';");
            migrationBuilder.Sql("IF COL_LENGTH('Exercises', 'PrimaryMuscleIds') IS NULL ALTER TABLE [Exercises] ADD [PrimaryMuscleIds] nvarchar(max) NOT NULL DEFAULT N'';");
            migrationBuilder.Sql("IF COL_LENGTH('Exercises', 'SecondaryMuscleIds') IS NULL ALTER TABLE [Exercises] ADD [SecondaryMuscleIds] nvarchar(max) NOT NULL DEFAULT N'';");
        }
    }
}
