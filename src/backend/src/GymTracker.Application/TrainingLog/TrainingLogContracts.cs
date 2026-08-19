namespace GymTracker.Application.TrainingLog;


public sealed record CreateTrainingEntryRequest(
    int WeekNumber,
    int DayNumber,
    string ExerciseCode,
    IReadOnlyCollection<CreateTrainingMetricRequest> Metrics
);


public sealed record UpdateTrainingLogValueRequest(
    int WeekNumber,
    int DayNumber,
    string ExerciseCode,
    IReadOnlyCollection<CreateTrainingMetricRequest> Metrics
);


public sealed record TrainingLogResponse(
     Guid Id,
    Guid GroupId,
    int WeekNumber,
    int DayNumber,
    string ExerciseCode,
    string UnitCode,
    decimal Value,
    DateTimeOffset Timestamp,
    Guid UserId);

public sealed record CreateTrainingMetricRequest(
    string Code,
    decimal Value
);


public sealed record TrainingLogGroupResponse(
    Guid GroupId,
    int WeekNumber,
    int DayNumber,
    string ExerciseId,
    DateTimeOffset Timestamp,
    IReadOnlyCollection<TrainingLogMetricResponse> Metrics
);

public sealed record TrainingLogMetricResponse(
    Guid Id,
    string UnitCode,
    decimal Value
);

public sealed record TrainingLogFilterRequest(
    string? RoutineId,
    string? SessionId,
    string? ExerciseId,
    DateOnly? Date
);