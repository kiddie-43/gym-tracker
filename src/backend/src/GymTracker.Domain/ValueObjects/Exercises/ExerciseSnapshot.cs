namespace GymTracker.Domain.ValueObjects;

public sealed record ExerciseSnapshot(
    string ExerciseId,
    string Name,
    string? CoverStoragePath,
    string FormTypeId,
    string FormTypeCode,
    DateTimeOffset CapturedAt);
