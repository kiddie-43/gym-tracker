using FluentAssertions;

using GymTracker.Domain.Services;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class ExerciseMediaInvariantsTests
{
    [Fact]
    public void ValidateCollection_ShouldThrow_WhenMultiplePrimaryItemsExist()
    {
        var now = DateTimeOffset.UtcNow;
        var items = new[]
        {
            CreateMedia("m1", ExerciseMediaType.Image, sortOrder: 0, isPrimary: true, now),
            CreateMedia("m2", ExerciseMediaType.Image, sortOrder: 1, isPrimary: true, now),
        };

        var action = () => ExerciseMediaRules.ValidateCollection(items);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*primary*");
    }

    [Fact]
    public void ValidateCollection_ShouldThrow_WhenSortOrderIsDuplicated()
    {
        var now = DateTimeOffset.UtcNow;
        var items = new[]
        {
            CreateMedia("m1", ExerciseMediaType.Image, sortOrder: 1, isPrimary: true, now),
            CreateMedia("m2", ExerciseMediaType.Video, sortOrder: 1, isPrimary: false, now),
        };

        var action = () => ExerciseMediaRules.ValidateCollection(items);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*sortOrder*");
    }

    [Fact]
    public void ValidateCollection_ShouldThrow_WhenImageLimitIsExceeded()
    {
        var now = DateTimeOffset.UtcNow;
        var items = Enumerable.Range(0, 7)
            .Select(index => CreateMedia($"img-{index}", ExerciseMediaType.Image, index, index == 0, now))
            .ToArray();

        var action = () => ExerciseMediaRules.ValidateCollection(items);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*6 active images*");
    }

    [Fact]
    public void ValidateCollection_ShouldPass_WhenLimitsAndInvariantsAreSatisfied()
    {
        var now = DateTimeOffset.UtcNow;
        var items = new[]
        {
            CreateMedia("img-1", ExerciseMediaType.Image, 0, true, now),
            CreateMedia("img-2", ExerciseMediaType.Image, 1, false, now),
            CreateMedia("vid-1", ExerciseMediaType.Video, 2, false, now),
        };

        var action = () => ExerciseMediaRules.ValidateCollection(items);

        action.Should().NotThrow();
    }

    private static ExerciseMedia CreateMedia(string id, ExerciseMediaType type, int sortOrder, bool isPrimary, DateTimeOffset now)
    {
        return ExerciseMedia.Create(
            id,
            type,
            title: id,
            storagePath: $"admin/exercises/ex-1/{id}/asset",
            thumbnailPath: null,
            contentType: type == ExerciseMediaType.Image ? "image/webp" : "video/mp4",
            fileName: $"{id}.bin",
            sizeBytes: 1000,
            sortOrder: sortOrder,
            isPrimary: isPrimary,
            now: now);
    }
}
