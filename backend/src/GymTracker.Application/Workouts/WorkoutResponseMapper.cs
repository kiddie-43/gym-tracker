using GymTracker.Domain.Entities;

namespace GymTracker.Application.Workouts;

internal static class WorkoutResponseMapper
{
    public static WorkoutResponse ToResponse(Workout workout)
    {
        return new WorkoutResponse(
            workout.Id,
            workout.PerformedAt,
            workout.Status.ToString().ToLowerInvariant(),
            workout.RoutineId,
            workout.Notes,
            workout.ExerciseEntries.Select(ToExerciseEntryResponse).ToArray());
    }

    private static WorkoutExerciseEntryResponse ToExerciseEntryResponse(ExerciseEntry entry)
    {
        return new WorkoutExerciseEntryResponse(
            entry.ExternalExerciseId,
            entry.ExerciseNameSnapshot,
            entry.ExerciseNameSnapshot,
            entry.MuscleGroupIds,
            entry.Sets.Select(setEntry => new WorkoutSetResponse(
                setEntry.Repetitions,
                setEntry.Weight,
                setEntry.RestSeconds,
                setEntry.Completed)).ToArray(),
            entry.Notes,
            entry.ImageUrl);
    }
}
