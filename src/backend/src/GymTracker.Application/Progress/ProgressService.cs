using GymTracker.Domain.Entities;
using GymTracker.Domain.Services;

using GymTracker.Application.Workouts;

namespace GymTracker.Application.Progress;

public sealed class ProgressService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IProgressSnapshotRepository _progressSnapshotRepository;
    private readonly ProgressCalculationService _progressCalculationService;

    public ProgressService(
        IWorkoutRepository workoutRepository,
        IProgressSnapshotRepository progressSnapshotRepository,
        ProgressCalculationService progressCalculationService)
    {
        _workoutRepository = workoutRepository;
        _progressSnapshotRepository = progressSnapshotRepository;
        _progressCalculationService = progressCalculationService;
    }

    public async Task<ProgressResponse> GetByExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var snapshot = await _progressSnapshotRepository.GetLatestAsync(userId, exerciseId, cancellationToken);
        if (snapshot is null)
        {
            var workouts = await _workoutRepository.ListByUserAndExerciseAsync(userId, exerciseId, cancellationToken);
            var result = _progressCalculationService.Calculate(exerciseId, workouts, DateTimeOffset.UtcNow);
            snapshot = new ProgressSnapshot
            {
                UserId = userId,
                ExerciseId = exerciseId,
                WorkoutId = workouts.OrderByDescending(workout => workout.PerformedAt).FirstOrDefault()?.Id ?? string.Empty,
                Result = result,
            };

            if (!string.IsNullOrWhiteSpace(snapshot.WorkoutId))
            {
                await _progressSnapshotRepository.SaveAsync(snapshot, cancellationToken);
            }
        }

        return Map(snapshot);
    }

    private static ProgressResponse Map(ProgressSnapshot snapshot)
    {
        return new ProgressResponse(
            snapshot.ExerciseId,
            snapshot.Result.Trend.ToString(),
            Map(snapshot.Result.Current),
            snapshot.Result.LastSessionComparison is null ? null : Map(snapshot.Result.LastSessionComparison),
            snapshot.Result.RollingAverageComparison is null ? null : Map(snapshot.Result.RollingAverageComparison),
            snapshot.Result.BestRecentComparison is null ? null : Map(snapshot.Result.BestRecentComparison),
            snapshot.Result.CalculatedAt);
    }

    private static ProgressMetricsResponse Map(ProgressMetrics metrics)
    {
        return new ProgressMetricsResponse(metrics.Volume, metrics.Load, metrics.Repetitions);
    }

    private static MetricDeltaResponse Map(MetricDelta delta)
    {
        return new MetricDeltaResponse(
            Map(delta.Reference),
            new MetricChangeResponse(delta.Volume.Current, delta.Volume.Reference, delta.Volume.PercentageChange),
            new MetricChangeResponse(delta.Load.Current, delta.Load.Reference, delta.Load.PercentageChange),
            new MetricChangeResponse(delta.Repetitions.Current, delta.Repetitions.Reference, delta.Repetitions.PercentageChange));
    }
}
