using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Admin.Exercises;

public static class ExerciseSnapshotMapper
{
    public static ExerciseSnapshot ToSnapshot(Exercise exercise, DateTimeOffset capturedAt)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return new ExerciseSnapshot(
            ExerciseId: exercise.Id,
            Name: exercise.Name,
            CoverStoragePath: exercise.ResolveCoverStoragePath(),
            FormTypeId: exercise.FormTypeId,
            FormTypeCode: exercise.FormTypeCode,
            CapturedAt: capturedAt);
    }
}
