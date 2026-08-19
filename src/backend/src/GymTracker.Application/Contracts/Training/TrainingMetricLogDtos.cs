using System.Text.Json.Serialization;

namespace GymTracker.Application.Contracts.Training;

public sealed record MetricDefinitionDto(
    string MetricId,
    string Name,
    string DataType,
    string? Unit);

public sealed record TrainingMetricLogDto(
    string LogId,
    string UserId,
    string RoutineId,
    string SessionId,
    string TrainingId,
    string ExerciseId,
    string MetricId,
    string GroupId,
    DateOnly Date,
    decimal Value,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record GroupMetricValueInputDto(
    string MetricId,
    decimal Value);

public sealed record CreateTrainingMetricGroupRequestDto(
    string RoutineId,
    string SessionId,
    string TrainingId,
    string ExerciseId,
    DateOnly Date,
    IReadOnlyCollection<GroupMetricValueInputDto> Metrics);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateMetricValueRequestDto(decimal Value);

public sealed record TrainingMetricGroupResponseDto(
    string GroupId,
    IReadOnlyCollection<TrainingMetricLogDto> Items);

public sealed record TrainingMetricLogsPageResponseDto(
    IReadOnlyCollection<TrainingMetricLogDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed record TrainingMetricLogsQueryDto(
    string? RoutineId,
    string? SessionId,
    string? TrainingId,
    string? ExerciseId,
    string? MetricId,
    int Page,
    int PageSize);