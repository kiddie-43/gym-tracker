namespace GymTracker.Application.Workouts;

public static class WorkoutValidators
{
    public static IReadOnlyCollection<string> Validate(CreateWorkoutRequest request)
    {
        return ValidateWorkoutRequest(request.PerformedAt, request.Status, request.ExerciseEntries);
    }

    public static IReadOnlyCollection<string> Validate(UpdateWorkoutRequest request)
    {
        return ValidateWorkoutRequest(request.PerformedAt, request.Status, request.ExerciseEntries);
    }

    private static IReadOnlyCollection<string> ValidateWorkoutRequest(DateTimeOffset performedAt, string status, IReadOnlyCollection<CreateExerciseEntryRequest> exerciseEntries)
    {
        var errors = new List<string>();

        if (performedAt == default)
        {
            errors.Add("performedAt is required.");
        }

        if (!string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, "incomplete", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("status must be 'completed' or 'incomplete'.");
        }

        if (exerciseEntries.Count == 0)
        {
            errors.Add("At least one exercise entry is required.");
        }

        foreach (var entry in exerciseEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.ExternalExerciseId))
            {
                errors.Add("externalExerciseId is required for each exercise entry.");
            }

            if (entry.Sets.Count == 0)
            {
                errors.Add($"At least one set is required for exercise {entry.ExternalExerciseId}.");
            }
        }

        return errors;
    }
}
