namespace GymTracker.Application.TrainingSession;

public sealed record CreateSessionBlockRequest(
    string BlockType,
    string Name,
    decimal DurationValue,
    string DurationUnitCode,
    string? Description,
    int? IntensityRpe,
    int OrderIndex
);

public sealed record CreateTrainingSessionRequest(
    int WeekNumber,
    int DayNumber,
    Guid ExerciseId,
    decimal DurationMinutes,
    decimal SecondaryMetricValue,
    string SecondaryMetricUnitCode,
    decimal TertiaryMetricValue,
    string? Notes,
    IReadOnlyCollection<CreateSessionBlockRequest> Blocks
);

public sealed record SessionBlockResponse(
    Guid Id,
    string BlockType,
    string Name,
    decimal DurationValue,
    string DurationUnitCode,
    string? Description,
    int? IntensityRpe,
    int OrderIndex
);

public sealed record TrainingSessionResponse(
    Guid Id,
    int WeekNumber,
    int DayNumber,
    Guid ExerciseId,
    DateTimeOffset Timestamp,
    decimal DurationMinutes,
    decimal SecondaryMetricValue,
    string SecondaryMetricUnitCode,
    decimal TertiaryMetricValue,
    string? Notes,
    IReadOnlyCollection<SessionBlockResponse> Blocks
);

public sealed record BlockTemplateItemResponse(
    string BlockType,
    string Name,
    decimal DefaultDurationMinutes,
    int OrderIndex
);
