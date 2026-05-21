using GymTracker.Application.Contracts.Training;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.Features.Training;

public sealed class ProgressComparisonService
{
    public ProgressComparisonDto BuildComparison(string exerciseId, IReadOnlyCollection<ExerciseTrainingLog> logs)
    {
        var ordered = logs.OrderByDescending(log => log.CreatedAt).ToArray();
        if (ordered.Length == 0)
        {
            throw new ArgumentException("No logs available for exercise.");
        }

        var current = BuildBaseline("lastSession", ordered[0]);
        var averageLast4 = BuildAverageBaseline("averageLast4", ordered.Take(4).ToArray());
        var bestRecent = BuildBaseline("bestRecent", ordered.OrderByDescending(GetVolume).First());

        var baselines = new[] { BuildBaseline("lastSession", ordered[0]), averageLast4, bestRecent };
        var variation = averageLast4.VolumeTotal == 0 ? 0 : ((current.VolumeTotal - averageLast4.VolumeTotal) / averageLast4.VolumeTotal) * 100;

        var trend = variation > 2 ? "improves" : variation < -2 ? "declines" : "stable";

        return new ProgressComparisonDto(exerciseId, current, baselines, decimal.Round(variation, 2), trend);
    }

    private static ProgressBaselineDto BuildBaseline(string label, ExerciseTrainingLog log)
    {
        var repetitions = log.PerformedSets.Sum(setItem => setItem.Repetitions);
        var loadTotal = log.PerformedSets.Sum(setItem => setItem.WeightKg);
        var volumeTotal = GetVolume(log);

        return new ProgressBaselineDto(label, volumeTotal, loadTotal, repetitions);
    }

    private static ProgressBaselineDto BuildAverageBaseline(string label, IReadOnlyCollection<ExerciseTrainingLog> logs)
    {
        var count = logs.Count;
        var volume = count == 0 ? 0 : logs.Average(GetVolume);
        var load = count == 0 ? 0 : logs.Average(log => log.PerformedSets.Sum(setItem => setItem.WeightKg));
        var repetitions = count == 0 ? 0 : (int)logs.Average(log => log.PerformedSets.Sum(setItem => setItem.Repetitions));

        return new ProgressBaselineDto(label, decimal.Round((decimal)volume, 2), decimal.Round((decimal)load, 2), repetitions);
    }

    private static decimal GetVolume(ExerciseTrainingLog log)
    {
        return log.PerformedSets.Sum(setItem => setItem.WeightKg * setItem.Repetitions);
    }
}
