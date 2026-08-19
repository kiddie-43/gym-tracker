namespace GymTracker.Application.TrainingSession;

using GymTracker.Domain.Enum;

/// <summary>
/// Static, in-memory reference content (INT-004): one default block structure per exercise profile,
/// used to prefill a training session's blocks when the user picks "Ver plantillas".
/// </summary>
public static class BlockTemplateCatalog
{
    private static readonly IReadOnlyCollection<BlockTemplateItemResponse> StrengthTemplate =
    [
        new(SessionBlockType.WARMUP.ToString(), "Calentamiento", 5m, 0),
        new(SessionBlockType.APPROACH.ToString(), "Series de aproximación", 10m, 1),
        new(SessionBlockType.WORK.ToString(), "Series de trabajo", 20m, 2),
        new(SessionBlockType.REST_STRENGTH.ToString(), "Descanso final", 5m, 3),
    ];

    private static readonly IReadOnlyCollection<BlockTemplateItemResponse> CardioTemplate =
    [
        new(SessionBlockType.SWIM.ToString(), "Calentamiento", 5m, 0),
        new(SessionBlockType.SERIES.ToString(), "Parte principal", 30m, 1),
        new(SessionBlockType.REST_CARDIO.ToString(), "Vuelta a la calma", 5m, 2),
    ];

    public static IReadOnlyCollection<BlockTemplateItemResponse> GetTemplate(ExerciseType exerciseType)
    {
        var profile = ExerciseTypeProfileMapper.ResolveProfile(exerciseType);

        return profile switch
        {
            ExerciseProfile.Strength => StrengthTemplate,
            ExerciseProfile.Cardio => CardioTemplate,
            _ => throw new ArgumentOutOfRangeException(nameof(exerciseType), exerciseType, "Unknown ExerciseType."),
        };
    }
}
