namespace GymTracker.Application.TrainingSession;

using GymTracker.Domain.Enum;

public enum ExerciseProfile
{
    Strength,
    Cardio,
}

public static class ExerciseTypeProfileMapper
{
    public static ExerciseProfile ResolveProfile(ExerciseType exerciseType) => exerciseType switch
    {
        ExerciseType.STRENGTH or ExerciseType.BODYWEIGHT or ExerciseType.PLYOMETRIC or ExerciseType.REHABILITATION
            => ExerciseProfile.Strength,
        ExerciseType.CARDIO or ExerciseType.MOBILITY or ExerciseType.STRETCHING or ExerciseType.SPORTS
            => ExerciseProfile.Cardio,
        _ => throw new ArgumentOutOfRangeException(nameof(exerciseType), exerciseType, "Unknown ExerciseType."),
    };

    public static IReadOnlyCollection<SessionBlockType> GetValidBlockTypes(ExerciseProfile profile) => profile switch
    {
        ExerciseProfile.Strength =>
        [
            SessionBlockType.WARMUP,
            SessionBlockType.APPROACH,
            SessionBlockType.WORK,
            SessionBlockType.REST_STRENGTH,
        ],
        ExerciseProfile.Cardio =>
        [
            SessionBlockType.SWIM,
            SessionBlockType.SERIES,
            SessionBlockType.TECHNIQUE,
            SessionBlockType.REST_CARDIO,
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown ExerciseProfile."),
    };

    public static bool IsValidForProfile(SessionBlockType blockType, ExerciseProfile profile) =>
        GetValidBlockTypes(profile).Contains(blockType);
}
