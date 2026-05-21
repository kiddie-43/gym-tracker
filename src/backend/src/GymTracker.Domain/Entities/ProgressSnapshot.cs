namespace GymTracker.Domain.Entities;

public sealed class ProgressSnapshot : BaseEntity
{
    public string UserId { get; init; } = string.Empty;

    public string ExerciseId { get; init; } = string.Empty;

    public string WorkoutId { get; init; } = string.Empty;

    public ProgressComparisonResult Result { get; init; } = ProgressComparisonResult.NoReference(DateTimeOffset.UtcNow);
}

public sealed record ProgressComparisonResult(
    TrendStatus Trend,
    ProgressMetrics Current,
    MetricDelta? LastSessionComparison,
    MetricDelta? RollingAverageComparison,
    MetricDelta? BestRecentComparison,
    DateTimeOffset CalculatedAt)
{
    public static ProgressComparisonResult NoReference(DateTimeOffset calculatedAt, ProgressMetrics? current = null)
    {
        return new ProgressComparisonResult(
            TrendStatus.NoReference,
            current ?? ProgressMetrics.Empty,
            null,
            null,
            null,
            calculatedAt);
    }

    public static ProgressComparisonResult Incomplete(DateTimeOffset calculatedAt, ProgressMetrics current)
    {
        return new ProgressComparisonResult(
            TrendStatus.IncompleteSession,
            current,
            null,
            null,
            null,
            calculatedAt);
    }
}

public sealed record ProgressMetrics(decimal Volume, decimal Load, int Repetitions)
{
    public static readonly ProgressMetrics Empty = new(0m, 0m, 0);
}

public sealed record MetricDelta(ProgressMetrics Reference, MetricChange Volume, MetricChange Load, MetricChange Repetitions);

public sealed record MetricChange(decimal Current, decimal Reference, decimal PercentageChange);

public enum TrendStatus
{
    Improving = 0,
    Stable = 1,
    Declining = 2,
    NoReference = 3,
    IncompleteSession = 4,
}
