using GymTracker.Domain.Entities;

namespace GymTracker.Domain.Services;

public sealed class ProgressCalculationService
{
    public ProgressComparisonResult Calculate(string exerciseId, IReadOnlyCollection<Workout> workouts, DateTimeOffset calculatedAt)
    {
        var relevantEntries = workouts
            .Select(workout => new
            {
                Workout = workout,
                Entry = workout.ExerciseEntries.FirstOrDefault(entry => string.Equals(entry.ExternalExerciseId, exerciseId, StringComparison.OrdinalIgnoreCase)),
            })
            .Where(item => item.Entry is not null)
            .OrderByDescending(item => item.Workout.PerformedAt)
            .ToArray();

        if (relevantEntries.Length == 0)
        {
            return ProgressComparisonResult.NoReference(calculatedAt);
        }

        var current = relevantEntries[0];
        var currentMetrics = ToMetrics(current.Entry!);

        if (current.Workout.Status == WorkoutStatus.Incomplete)
        {
            return ProgressComparisonResult.Incomplete(calculatedAt, currentMetrics);
        }

        var historical = relevantEntries
            .Skip(1)
            .Where(item => item.Workout.Status == WorkoutStatus.Completed)
            .Select(item => ToMetrics(item.Entry!))
            .ToArray();

        if (historical.Length == 0)
        {
            return ProgressComparisonResult.NoReference(calculatedAt, currentMetrics);
        }

        var lastSession = BuildDelta(currentMetrics, historical[0]);
        var rollingAverage = BuildDelta(currentMetrics, Average(historical.Take(4).ToArray()));
        var bestRecent = BuildDelta(currentMetrics, Best(historical.Take(4).ToArray()));

        return new ProgressComparisonResult(
            DetermineTrend(lastSession),
            currentMetrics,
            lastSession,
            rollingAverage,
            bestRecent,
            calculatedAt);
    }

    private static ProgressMetrics ToMetrics(ExerciseEntry entry)
    {
        var completedSets = entry.Sets.Where(setEntry => setEntry.Completed).ToArray();
        var repetitions = completedSets.Sum(setEntry => setEntry.Repetitions);
        var volume = completedSets.Sum(setEntry => setEntry.Repetitions * (setEntry.Weight ?? 0m));
        var weightedSets = completedSets.Where(setEntry => setEntry.Weight.HasValue).ToArray();
        var load = weightedSets.Length == 0 ? 0m : weightedSets.Average(setEntry => setEntry.Weight ?? 0m);

        return new ProgressMetrics(volume, load, repetitions);
    }

    private static MetricDelta BuildDelta(ProgressMetrics current, ProgressMetrics reference)
    {
        return new MetricDelta(
            reference,
            BuildChange(current.Volume, reference.Volume),
            BuildChange(current.Load, reference.Load),
            BuildChange(current.Repetitions, reference.Repetitions));
    }

    private static MetricChange BuildChange(decimal current, decimal reference)
    {
        var percentageChange = reference == 0m
            ? (current == 0m ? 0m : 100m)
            : decimal.Round(((current - reference) / reference) * 100m, 2, MidpointRounding.AwayFromZero);

        return new MetricChange(current, reference, percentageChange);
    }

    private static MetricChange BuildChange(int current, int reference)
    {
        var percentageChange = reference == 0
            ? (current == 0 ? 0m : 100m)
            : decimal.Round(((current - reference) / (decimal)reference) * 100m, 2, MidpointRounding.AwayFromZero);

        return new MetricChange(current, reference, percentageChange);
    }

    private static ProgressMetrics Average(IReadOnlyCollection<ProgressMetrics> metrics)
    {
        return new ProgressMetrics(
            metrics.Average(item => item.Volume),
            metrics.Average(item => item.Load),
            (int)Math.Round(metrics.Average(item => item.Repetitions), MidpointRounding.AwayFromZero));
    }

    private static ProgressMetrics Best(IReadOnlyCollection<ProgressMetrics> metrics)
    {
        return new ProgressMetrics(
            metrics.Max(item => item.Volume),
            metrics.Max(item => item.Load),
            metrics.Max(item => item.Repetitions));
    }

    private static TrendStatus DetermineTrend(MetricDelta lastSession)
    {
        var improvingSignals = 0;
        var decliningSignals = 0;

        foreach (var change in new[] { lastSession.Volume.PercentageChange, lastSession.Load.PercentageChange, lastSession.Repetitions.PercentageChange })
        {
            if (change > 2m)
            {
                improvingSignals++;
            }
            else if (change < -2m)
            {
                decliningSignals++;
            }
        }

        if (improvingSignals > decliningSignals)
        {
            return TrendStatus.Improving;
        }

        if (decliningSignals > improvingSignals)
        {
            return TrendStatus.Declining;
        }

        return TrendStatus.Stable;
    }
}
