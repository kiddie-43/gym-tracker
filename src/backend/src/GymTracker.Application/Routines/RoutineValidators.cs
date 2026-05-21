namespace GymTracker.Application.Routines;

public static class RoutineValidators
{
    public static IReadOnlyCollection<string> Validate(CreateRoutineRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("name is required.");
        }

        if (request.Days.Count == 0)
        {
            errors.Add("At least one routine day is required.");
        }

        var duplicateDays = request.Days
            .GroupBy(day => day.DayLabel.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateDays.Length > 0)
        {
            errors.Add($"Duplicate day labels are not allowed: {string.Join(", ", duplicateDays)}.");
        }

        foreach (var day in request.Days)
        {
            if (string.IsNullOrWhiteSpace(day.DayLabel))
            {
                errors.Add("dayLabel is required for each day.");
            }

            if (day.Exercises.Count == 0)
            {
                errors.Add($"At least one exercise is required for day {day.DayLabel}.");
                continue;
            }

            foreach (var exercise in day.Exercises)
            {
                if (string.IsNullOrWhiteSpace(exercise.ExternalExerciseId))
                {
                    errors.Add($"externalExerciseId is required in day {day.DayLabel}.");
                }

                if (exercise.TargetSets <= 0)
                {
                    errors.Add($"targetSets must be greater than zero in day {day.DayLabel}.");
                }

                if (exercise.TargetRepetitions <= 0)
                {
                    errors.Add($"targetRepetitions must be greater than zero in day {day.DayLabel}.");
                }
            }
        }

        return errors;
    }
}
