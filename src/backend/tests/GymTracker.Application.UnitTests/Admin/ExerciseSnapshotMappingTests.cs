using FluentAssertions;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class ExerciseSnapshotMappingTests
{
    [Fact]
    public void ToSnapshot_ShouldMapCoreFieldsAndPrimaryCover()
    {
        var now = DateTimeOffset.Parse("2026-05-11T10:00:00Z");
        var exercise = Exercise.Create(
            name: "Press banca",
            code: "BENCH_PRESS",
            description: "Ejercicio de pecho",
            category: "type-strength",
            difficulty: "STRENGTH",
            measurementTypeIds: new[] { "form-strength" },
            measurementTypeCode: "STRENGTH_BASIC",
            primaryMuscleIds: new[] { "chest" },
            secondaryMuscleIds: new[] { "triceps" },
            muscleGroupIds: new[] { "upper-body" },
            media: new[]
            {
                ExerciseMedia.Create(
                    mediaId: "m1",
                    mediaType: ExerciseMediaType.Image,
                    title: "Frontal",
                    storagePath: "admin/exercises/e1/m1/front.webp",
                    thumbnailPath: null,
                    contentType: "image/webp",
                    fileName: "front.webp",
                    sizeBytes: 1024,
                    sortOrder: 0,
                    isPrimary: true,
                    now: now),
            });

        var snapshot = ExerciseSnapshotMapper.ToSnapshot(exercise, now);

        snapshot.ExerciseId.Should().Be(exercise.Id);
        snapshot.Name.Should().Be("Press banca");
        snapshot.CoverStoragePath.Should().Be("admin/exercises/e1/m1/front.webp");
        snapshot.FormTypeId.Should().Be("form-strength");
        snapshot.FormTypeCode.Should().Be("STRENGTH_BASIC");
        snapshot.CapturedAt.Should().Be(now);
    }
}
