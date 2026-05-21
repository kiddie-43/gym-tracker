using FluentAssertions;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Api.IntegrationTests.Workouts;

public sealed class WorkoutSnapshotTests
{
    [Fact]
    public void SnapshotMapper_ShouldCaptureExerciseSnapshotForWorkoutUsage()
    {
        var exercise = Exercise.Create(
            name: "Peso muerto",
            code: "DEADLIFT",
            description: "Cadena posterior",
            category: "type-strength",
            difficulty: "STRENGTH",
            measurementTypeIds: new[] { "form-strength" },
            measurementTypeCode: "STRENGTH_BASIC",
            primaryMuscleIds: new[] { "back" },
            secondaryMuscleIds: new[] { "legs" },
            muscleGroupIds: new[] { "posterior-chain" },
            media: new[]
            {
                ExerciseMedia.Create("m1", ExerciseMediaType.Image, "Cover", "admin/exercises/e1/m1/cover.webp", null, "image/webp", "cover.webp", 1024, 0, true, DateTimeOffset.UtcNow),
            });

        var snapshot = ExerciseSnapshotMapper.ToSnapshot(exercise, DateTimeOffset.UtcNow);

        snapshot.ExerciseId.Should().Be(exercise.Id);
        snapshot.FormTypeCode.Should().Be("STRENGTH_BASIC");
        snapshot.CoverStoragePath.Should().NotBeNullOrWhiteSpace();
    }
}
