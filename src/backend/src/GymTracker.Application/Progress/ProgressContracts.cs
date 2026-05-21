namespace GymTracker.Application.Progress;

public sealed record ProgressResponse(
    string ExerciseId,
    string Trend,
    ProgressMetricsResponse Current,
    MetricDeltaResponse? LastSessionComparison,
    MetricDeltaResponse? RollingAverageComparison,
    MetricDeltaResponse? BestRecentComparison,
    DateTimeOffset CalculatedAt);

public sealed record ProgressMetricsResponse(decimal Volume, decimal Load, int Repetitions);

public sealed record MetricDeltaResponse(
    ProgressMetricsResponse Reference,
    MetricChangeResponse Volume,
    MetricChangeResponse Load,
    MetricChangeResponse Repetitions);

public sealed record MetricChangeResponse(decimal Current, decimal Reference, decimal PercentageChange);
