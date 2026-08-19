namespace GymTracker.Application.Contracts.Training;

public sealed record StartTrainingFlowDto(string RoutineId);

public sealed record UpdateTrainingFlowDto(string? RoutineId, string? SessionId, string? ExerciseId, string StepNode);

public sealed record TrainingFlowStateDto(bool IsLocked, string RoutineId, string? SessionId, string? ExerciseId, string StepNode, DateTimeOffset LastUpdatedAt);

public sealed record PerformedSetDto(int Repetitions, decimal WeightKg, int Order);

public sealed record TrainingAttachmentDto(string Type, string Url);

public sealed record CreateExerciseTrainingLogDto(
    string RoutineId,
    string SessionId,
    string ExerciseId,
    IReadOnlyCollection<PerformedSetDto> PerformedSets,
    string? Notes,
    IReadOnlyCollection<TrainingAttachmentDto>? Attachments);

public sealed record ExerciseTrainingLogDto(
    string Id,
    string UserId,
    string RoutineId,
    string SessionId,
    string ExerciseId,
    IReadOnlyCollection<PerformedSetDto> PerformedSets,
    string? Notes,
    IReadOnlyCollection<TrainingAttachmentDto> Attachments,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ProgressBaselineDto(string Label, decimal VolumeTotal, decimal LoadTotal, int RepetitionsTotal);

public sealed record ProgressComparisonDto(
    string ExerciseId,
    ProgressBaselineDto Current,
    IReadOnlyCollection<ProgressBaselineDto> Baselines,
    decimal VariationPercent,
    string Trend);
