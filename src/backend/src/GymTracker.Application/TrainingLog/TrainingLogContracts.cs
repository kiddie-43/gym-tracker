namespace GymTracker.Application.TrainingLog;


public sealed record CreateTrainingEntryRequest(
    string RoutineId,
    string SessionId,
    string ExerciseId,
    IReadOnlyCollection<CreateTrainingMetricRequest> Metrics
);


public sealed record UpdateTrainingLogValueRequest(
    decimal Value
);


public sealed record TrainingLogResponse(
     Guid Id,
    Guid GroupId,
    string RoutineId,
    string SessionId,
    string ExerciseId,
    string MetricId,
    decimal Value,
    DateTimeOffset Timestamp,
    Guid UserId);

public sealed record CreateTrainingMetricRequest(
    string MetricId,
    decimal Value
);


public sealed record TrainingLogGroupResponse(
    Guid GroupId,
    string RoutineId,
    string SessionId,
    string ExerciseId,
    DateTimeOffset Timestamp,
    IReadOnlyCollection<TrainingLogMetricResponse> Metrics
);

public sealed record TrainingLogMetricResponse(
    Guid Id,
    string MetricId,
    decimal Value
);

public sealed record TrainingLogFilterRequest(
    string? RoutineId,
    string? SessionId,
    string? ExerciseId,
    DateOnly? Date
);