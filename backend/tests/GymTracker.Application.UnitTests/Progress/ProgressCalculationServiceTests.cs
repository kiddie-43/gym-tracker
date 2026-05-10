using FluentAssertions;

using GymTracker.Domain.Entities;
using GymTracker.Domain.Services;

namespace GymTracker.Application.UnitTests.Progress;

public sealed class ProgressCalculationServiceTests
{
    private readonly ProgressCalculationService _service = new();

    [Fact]
    public void Calculate_ShouldCompareAgainstLastSessionRollingAverageAndBestRecent_WhenHistoryExists()
    {
        var workouts = new[]
        {
            CreateWorkout("w5", DateTimeOffset.Parse("2026-05-05T10:00:00Z"), WorkoutStatus.Completed, 100m, 8),
            CreateWorkout("w4", DateTimeOffset.Parse("2026-05-01T10:00:00Z"), WorkoutStatus.Completed, 90m, 8),
            CreateWorkout("w3", DateTimeOffset.Parse("2026-04-28T10:00:00Z"), WorkoutStatus.Completed, 85m, 8),
            CreateWorkout("w2", DateTimeOffset.Parse("2026-04-25T10:00:00Z"), WorkoutStatus.Completed, 80m, 8),
            CreateWorkout("w1", DateTimeOffset.Parse("2026-04-20T10:00:00Z"), WorkoutStatus.Completed, 75m, 8),
        };

        var result = _service.Calculate("bench-press", workouts, DateTimeOffset.Parse("2026-05-05T10:05:00Z"));

        result.Trend.Should().Be(TrendStatus.Improving);
        result.LastSessionComparison.Should().NotBeNull();
        result.LastSessionComparison!.Volume.Reference.Should().Be(720m);
        result.LastSessionComparison.Volume.Current.Should().Be(800m);
        result.RollingAverageComparison.Should().NotBeNull();
        result.RollingAverageComparison!.Volume.Reference.Should().Be(660m);
        result.BestRecentComparison.Should().NotBeNull();
        result.BestRecentComparison!.Volume.Reference.Should().Be(720m);
    }

    [Fact]
    public void Calculate_ShouldReturnNoReference_WhenThereIsNoPreviousEquivalentSession()
    {
        var workouts = new[]
        {
            CreateWorkout("w1", DateTimeOffset.Parse("2026-05-05T10:00:00Z"), WorkoutStatus.Completed, 100m, 8),
        };

        var result = _service.Calculate("bench-press", workouts, DateTimeOffset.Parse("2026-05-05T10:05:00Z"));

        result.Trend.Should().Be(TrendStatus.NoReference);
        result.LastSessionComparison.Should().BeNull();
        result.RollingAverageComparison.Should().BeNull();
        result.BestRecentComparison.Should().BeNull();
        result.Current.Volume.Should().Be(800m);
    }

    [Fact]
    public void Calculate_ShouldFlagIncompleteSession_WhenCurrentWorkoutIsIncomplete()
    {
        var workouts = new[]
        {
            CreateWorkout("w2", DateTimeOffset.Parse("2026-05-05T10:00:00Z"), WorkoutStatus.Incomplete, 100m, 8),
            CreateWorkout("w1", DateTimeOffset.Parse("2026-05-01T10:00:00Z"), WorkoutStatus.Completed, 90m, 8),
        };

        var result = _service.Calculate("bench-press", workouts, DateTimeOffset.Parse("2026-05-05T10:05:00Z"));

        result.Trend.Should().Be(TrendStatus.IncompleteSession);
        result.LastSessionComparison.Should().BeNull();
        result.RollingAverageComparison.Should().BeNull();
        result.BestRecentComparison.Should().BeNull();
        result.Current.Repetitions.Should().Be(8);
    }

    private static Workout CreateWorkout(string id, DateTimeOffset performedAt, WorkoutStatus status, decimal weight, int repetitions)
    {
        return new Workout
        {
            Id = id,
            UserId = "user-1",
            PerformedAt = performedAt,
            Status = status,
            ExerciseEntries = new[]
            {
                new ExerciseEntry
                {
                    ExternalExerciseId = "bench-press",
                    ExerciseNameSnapshot = "Bench press",
                    Sets = new[]
                    {
                        new WorkoutSet
                        {
                            Weight = weight,
                            Repetitions = repetitions,
                            Completed = true,
                        },
                    },
                },
            },
        };
    }
}