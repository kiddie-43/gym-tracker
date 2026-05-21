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
        var snapshot = entry.ExerciseSnapshot;

        return new WorkoutExerciseEntryResponse(
            entry.ExternalExerciseId,
            snapshot?.Name ?? entry.ExerciseNameSnapshot,
            entry.ExerciseNameSnapshot,
            entry.MuscleGroupIds,
            entry.Sets.Select(setEntry => new WorkoutSetResponse(
                setEntry.Repetitions,
                setEntry.Weight,
                setEntry.RestSeconds,
                setEntry.Completed)).ToArray(),
            entry.Notes,
            snapshot?.CoverStoragePath ?? entry.ImageUrl,
            snapshot is null
                ? null
                : new WorkoutExerciseSnapshotResponse(
                    snapshot.ExerciseId,
                    snapshot.Name,
                    snapshot.CoverStoragePath,
                    snapshot.FormTypeId,
                    snapshot.FormTypeCode,
                    snapshot.CapturedAt));
    }
}
