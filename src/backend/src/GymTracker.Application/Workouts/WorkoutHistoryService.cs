namespace GymTracker.Application.Workouts;

public sealed class WorkoutHistoryService
{
    private readonly IWorkoutRepository _workoutRepository;

    public WorkoutHistoryService(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<WorkoutHistoryPageResponse> GetHistoryAsync(
        string userId,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.ListByUserAsync(userId, cancellationToken);

        var query = workouts
            .Where(workout => !from.HasValue || DateOnly.FromDateTime(workout.PerformedAt.UtcDateTime) >= from.Value)
            .Where(workout => !to.HasValue || DateOnly.FromDateTime(workout.PerformedAt.UtcDateTime) <= to.Value)
            .OrderByDescending(workout => workout.PerformedAt);

        var total = query.Count();
        var items = query
            .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Select(WorkoutResponseMapper.ToResponse)
            .ToArray();

        return new WorkoutHistoryPageResponse(items, total, Math.Max(page, 1), Math.Max(pageSize, 1));
    }
}

public sealed record WorkoutHistoryPageResponse(
    IReadOnlyCollection<WorkoutResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
