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
                request.WeekNumber,
                request.DayNumber,
                request.ExerciseCode,
                metric.Code,
                metric.Value,
                timestamp,
                userId,
                groupId))
            .ToArray();

        await _repository.CreateGroupAsync(logs, cancellationToken);

        return MapGroup(logs);
    }

    public async Task<TrainingLogGroupResponse?> UpdateValueAsync(
        Guid groupId,
        UpdateTrainingLogValueRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var existingLogs = await _repository.ListByGroupIdAsync(groupId, cancellationToken);

        if (existingLogs.Count == 0)
            return null;

        var firstLog = existingLogs.First();
        if (firstLog.UserId != userId)
            throw new UnauthorizedAccessException();

        foreach (var log in existingLogs)
        {
            log.Delete(userId);
            await _repository.UpdateAsync(log, cancellationToken);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var newLogs = request.Metrics
            .Select(metric => TrainingLog.Create(
                request.WeekNumber,
                request.DayNumber,
                request.ExerciseCode,
                metric.Code,
                metric.Value,
                timestamp,
                userId,
                groupId))
            .ToArray();

        await _repository.CreateGroupAsync(newLogs, cancellationToken);

        return MapGroup(newLogs);
    }
public async Task<TrainingLogResponse?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    var log = await _repository.GetByIdAsync(id, cancellationToken);

    return log is null ? null : Map(log);
}

    public async Task<bool> DeleteAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _repository.ListByGroupIdAsync(groupId, cancellationToken);

        if (logs.Count == 0)
            return false;

        var firstLog = logs.First();
        if (firstLog.UserId != userId)
            throw new UnauthorizedAccessException();

        foreach (var log in logs)
        {
            log.Delete(userId);
            await _repository.UpdateAsync(log, cancellationToken);
        }

        return true;
    }
public async Task<IReadOnlyCollection<TrainingLogGroupResponse>> ListAsync(
    Guid userId,
    int weekNumber,
    int dayNumber,
    string exerciseCode,
    CancellationToken cancellationToken = default)
{
    var logs = await _repository.ListAsync(
        userId,
        weekNumber,
        dayNumber,
        exerciseCode,
        null,
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
            log.WeekNumber,
            log.DayNumber,
            log.ExerciseCode,
            log.UnitCode,
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
            first.WeekNumber,
            first.DayNumber,
            first.ExerciseCode,
            first.Timestamp,
            logs.Select(log => new TrainingLogMetricResponse(
                log.Id,
                log.UnitCode,
                log.Value)).ToArray());
    }
}