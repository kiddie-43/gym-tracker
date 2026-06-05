namespace GymTracker.Application.TrainingLog;

using GymTracker.Domain.Entities;

public sealed class TrainingLogService
{
    private readonly ITrainingLogRepository _repository;

    public TrainingLogService(ITrainingLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<TrainingLogGroupResponse> CreateAsync(
        CreateTrainingEntryRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var groupId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var logs = request.Metrics
            .Select(metric => TrainingLog.Create(
                request.RoutineId,
                request.SessionId,
                Guid.Parse(request.ExerciseId),
                Guid.Parse(metric.MetricId),
                metric.Value,
                timestamp,
                userId,
                groupId))
            .ToArray();

        await _repository.CreateGroupAsync(logs, cancellationToken);

        return MapGroup(logs);
    }

  public async Task<TrainingLogResponse?> UpdateValueAsync(
    Guid id,
    UpdateTrainingLogValueRequest request,
    Guid userId,
    CancellationToken cancellationToken = default)
{
    var log = await _repository.GetByIdAsync(id, cancellationToken);

    if (log is null)
        return null;

    if (log.UserId != userId)
        throw new UnauthorizedAccessException();

    log.UpdateValue(request.Value);

    await _repository.UpdateAsync(log, cancellationToken);

    return Map(log);
}
public async Task<TrainingLogResponse?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    var log = await _repository.GetByIdAsync(id, cancellationToken);

    return log is null ? null : Map(log);
}

public async Task<bool> DeleteAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    var log = await _repository.GetByIdAsync(id, cancellationToken);

    if (log is null)
        return false;

    await _repository.DeleteAsync(log, cancellationToken);

    return true;
}
public async Task<IReadOnlyCollection<TrainingLogGroupResponse>> ListAsync(
    Guid userId,
    string routineId,
    string sessionId,
    string exerciseId,
    CancellationToken cancellationToken = default)
{
    var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7);
    var logs = await _repository.ListAsync(
        userId,
        routineId,
        sessionId,
        exerciseId,
        date,
        cancellationToken);

    return logs
        .GroupBy(x => x.GroupId)
        .Select(group => MapGroup(group.ToArray()))
        .ToArray();
}
    private static TrainingLogResponse Map(TrainingLog log)
    {
        return new TrainingLogResponse(
            log.Id,
            log.GroupId,
            log.RoutineId,
            log.SessionId,
            log.ExerciseId.ToString(),
            log.MetricId.ToString(),
            log.Value,
            log.Timestamp,
            log.UserId);
    }

    private static TrainingLogGroupResponse MapGroup(
        IReadOnlyCollection<TrainingLog> logs)
    {
        var first = logs.First();

        return new TrainingLogGroupResponse(
            first.GroupId,
            first.RoutineId,
            first.SessionId,
            first.ExerciseId.ToString(),
            first.Timestamp,
            logs.Select(log => new TrainingLogMetricResponse(
                log.Id,
                log.MetricId.ToString(),
                log.Value)).ToArray());
    }
}