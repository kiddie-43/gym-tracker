namespace GymTracker.Domain.ValueObjects;

public enum ExerciseMediaType
{
    Image = 0,
    Video = 1,
}

public sealed record ExerciseMedia(
    string MediaId,
    ExerciseMediaType MediaType,
    bool Active,
    bool IsDeleted,
    bool IsPrimary,
    int SortOrder);
