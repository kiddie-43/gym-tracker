using FluentAssertions;

using GymTracker.Application.Features.Training;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.UnitTests.Training;

public sealed class ProgressComparisonServiceTests
{
    private readonly ProgressComparisonService _service = new();

    [Fact]
    public void BuildComparison_ShouldCalculateRequiredMetricsAndTrend()
    {
        var logs = new[]
        {
            BuildLog("exercise-1", DateTimeOffset.UtcNow.AddDays(-3), 8, 22),
            BuildLog("exercise-1", DateTimeOffset.UtcNow.AddDays(-2), 8, 24),
            BuildLog("exercise-1", DateTimeOffset.UtcNow.AddDays(-1), 8, 26),
            BuildLog("exercise-1", DateTimeOffset.UtcNow, 8, 28),
        };

        var result = _service.BuildComparison("exercise-1", logs);

        result.ExerciseId.Should().Be("exercise-1");
        result.Current.VolumeTotal.Should().Be(224);
        result.Current.LoadTotal.Should().Be(28);
        result.Current.RepetitionsTotal.Should().Be(8);
        result.Baselines.Should().HaveCount(3);
        result.VariationPercent.Should().BeGreaterThan(0);
        result.Trend.Should().Be("improves");
    }

    [Fact]
    public void BuildComparison_ShouldReturnStableTrend_WhenVariationNearZero()
    {
        var logs = new[]
        {
            BuildLog("exercise-1", DateTimeOffset.UtcNow.AddDays(-1), 10, 20),
            BuildLog("exercise-1", DateTimeOffset.UtcNow, 10, 20),
        };

        var result = _service.BuildComparison("exercise-1", logs);

        result.VariationPercent.Should().Be(0);
        result.Trend.Should().Be("stable");
    }

    private static ExerciseTrainingLog BuildLog(string exerciseId, DateTimeOffset createdAt, int repetitions, decimal weightKg)
    {
        return new ExerciseTrainingLog
        {
            UserId = "user-1",
            RoutineId = "routine-1",
            SessionId = "session-1",
            ExerciseId = exerciseId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            PerformedSets =
            {
                new PerformedSet
                {
                    Order = 1,
                    Repetitions = repetitions,
                    WeightKg = weightKg,
                },
            },
        };
    }
}
