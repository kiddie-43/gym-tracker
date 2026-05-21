using GymTracker.Domain.Entities.Routines;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.Validation;

public static class RoutinesValidationRules
{
    private static readonly HashSet<string> ValidDays = new(StringComparer.OrdinalIgnoreCase)
    {
        "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday",
    };

    public static void EnsureRoutineTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Routine title is required.");
        }
    }

    public static IReadOnlyCollection<string> NormalizeDays(IReadOnlyCollection<string> days)
    {
        if (days.Count == 0)
        {
            throw new ArgumentException("At least one training day is required.");
        }

        var normalized = days
            .Select(day => day.Trim().ToLowerInvariant())
            .ToList();

        if (normalized.Any(day => !ValidDays.Contains(day)))
        {
            throw new ArgumentException("Session days contain invalid values.");
        }

        if (normalized.Count != normalized.Distinct(StringComparer.Ordinal).Count())
        {
            throw new ArgumentException("Session days must be unique.");
        }

        return normalized;
    }

    public static void EnsureNoSessionDayConflict(IEnumerable<RoutineSession> activeSessions, IReadOnlyCollection<string> normalizedDays, string? excludeSessionId = null)
    {
        var inUse = activeSessions
            .Where(session => !session.IsDeleted)
            .Where(session => !string.Equals(session.Id, excludeSessionId, StringComparison.Ordinal))
            .SelectMany(session => session.DaysOfWeek)
            .ToHashSet(StringComparer.Ordinal);

        if (normalizedDays.Any(inUse.Contains))
        {
            throw new InvalidOperationException("Session day conflict detected for routine.");
        }
    }

    public static void EnsurePlannedSet(int repetitions, decimal weightKg)
    {
        if (repetitions <= 0 || weightKg <= 0)
        {
            throw new ArgumentException("Planned set requires positive repetitions and weight.");
        }
    }

    public static void EnsureExerciseLog(ExerciseTrainingLog log)
    {
        if (log.PerformedSets.Count == 0)
        {
            throw new ArgumentException("At least one performed set is required.");
        }

        if (log.PerformedSets.Any(setItem => setItem.Repetitions <= 0 || setItem.WeightKg <= 0))
        {
            throw new ArgumentException("Performed sets require positive repetitions and weight.");
        }

        if (log.Attachments.Count > 5)
        {
            throw new ArgumentException("A maximum of 5 attachments is allowed.");
        }
    }

    public static void EnsureTrainingStepNode(string stepNode)
    {
        var valid = new[] { "routine", "session", "exercise", "exerciseData" };
        if (!valid.Contains(stepNode, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid training flow step node.");
        }
    }
}
