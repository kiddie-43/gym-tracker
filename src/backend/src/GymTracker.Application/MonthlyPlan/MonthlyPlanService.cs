using GymTracker.Domain.Entities;
using System.Collections.Concurrent;

namespace GymTracker.Application.MonthlyPlan;

public sealed class MonthlyPlanService
{
    private static readonly ConcurrentDictionary<Guid, CachedMonthlyPlanState> LastConfirmedPlans = new();

    private sealed record CachedMonthlyPlanState(MonthlyPlanResponse Plan, DateTimeOffset ConfirmedAt);

    private readonly IMonthlyPlanRepository _repository;

    public MonthlyPlanService(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<MonthlyPlanResponse> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        try
        {
            var monthlyPlan = await _repository.GetByUserIdAsync(userId, cancellationToken);
            if (monthlyPlan is null)
            {
                monthlyPlan = Domain.Entities.MonthlyPlan.Create(userId, activeDays: 7);
                await _repository.AddAsync(monthlyPlan, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }

            monthlyPlan.EnsureStructure();

            var response = await MapAsync(monthlyPlan, cancellationToken);
            SaveAsLastConfirmed(userId, response);
            return response;
        }
        catch (Exception exception)
        {
            if (LastConfirmedPlans.TryGetValue(userId, out var cached))
            {
                return cached.Plan;
            }

            throw new InvalidOperationException("Monthly plan temporarily unavailable.", exception);
        }
    }

    public async Task<MonthlyPlanResponse> UpsertAsync(Guid userId, UpsertMonthlyPlanRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        ValidateUpsertRequest(request);

        var requestedExerciseIds = request.Days
            .SelectMany(day => day.Exercises)
            .Select(exercise => exercise.ExerciseId)
            .Distinct()
            .ToArray();

        if (requestedExerciseIds.Length > 0 && !await _repository.AllExercisesExistAsync(requestedExerciseIds, cancellationToken))
        {
            throw new ArgumentException("One or more exercises are invalid or deleted.", nameof(request));
        }

        try
        {
            var monthlyPlan = await _repository.GetByUserIdAsync(userId, cancellationToken);
            if (monthlyPlan is null)
            {
                monthlyPlan = Domain.Entities.MonthlyPlan.Create(userId, request.ActiveDays);
                await _repository.AddAsync(monthlyPlan, cancellationToken);
            }
            else
            {
                monthlyPlan.UpdateActiveDays(request.ActiveDays);
            }

            var planned = request.Days
                .OrderBy(day => day.WeekNumber)
                .ThenBy(day => day.DayNumber)
                .SelectMany(day => day.Exercises
                    .OrderBy(exercise => exercise.OrderIndex)
                    .Select((exercise, index) => PlannedExercise.Create(
                        monthlyPlan.Id,
                        userId,
                        day.WeekNumber,
                        day.DayNumber,
                        exercise.ExerciseId,
                        index)))
                .ToArray();

            monthlyPlan.ReplacePlannedExercises(planned);

            await _repository.UpdateAsync(monthlyPlan, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            monthlyPlan.EnsureStructure();

            var response = await MapAsync(monthlyPlan, cancellationToken);
            SaveAsLastConfirmed(userId, response);
            return response;
        }
        catch (Exception exception)
        {
            if (LastConfirmedPlans.TryGetValue(userId, out var cached))
            {
                return cached.Plan;
            }

            throw new InvalidOperationException("Monthly plan save failed.", exception);
        }
    }

    public async Task<MonthlyPlanResponse> TruncateDaysAsync(Guid userId, TruncateDaysRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (request.ActiveDays < 1 || request.ActiveDays > 7)
        {
            throw new ArgumentException("ActiveDays must be between 1 and 7.", nameof(request));
        }

        if (!request.Confirmed)
        {
            throw new ArgumentException("Truncation must be confirmed.", nameof(request));
        }

        try
        {
            var monthlyPlan = await _repository.GetByUserIdAsync(userId, cancellationToken);
            if (monthlyPlan is null)
            {
                monthlyPlan = Domain.Entities.MonthlyPlan.Create(userId, request.ActiveDays);
                await _repository.AddAsync(monthlyPlan, cancellationToken);
            }

            var affectedExercises = monthlyPlan.PlannedExercises
                .Where(x => x.DayNumber > request.ActiveDays)
                .ToArray();

            var historical = affectedExercises
                .Select(x => HistoricalExerciseRecord.Create(
                    x.Id,
                    userId,
                    x.WeekNumber,
                    x.DayNumber,
                    payload: "{}",
                    sourceReason: "truncate-days"))
                .ToArray();

            monthlyPlan.UpdateActiveDays(request.ActiveDays);

            var retainedExercises = monthlyPlan.PlannedExercises
                .Where(x => x.DayNumber <= request.ActiveDays)
                .ToArray();

            monthlyPlan.ReplacePlannedExercises(retainedExercises);

            if (historical.Length > 0)
            {
                await _repository.AddHistoricalRecordsAsync(historical, cancellationToken);
            }

            await _repository.UpdateAsync(monthlyPlan, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            monthlyPlan.EnsureStructure();

            var response = await MapAsync(monthlyPlan, cancellationToken);
            SaveAsLastConfirmed(userId, response);
            return response;
        }
        catch (Exception exception)
        {
            if (LastConfirmedPlans.TryGetValue(userId, out var cached))
            {
                return cached.Plan;
            }

            throw new InvalidOperationException("Monthly plan truncation failed.", exception);
        }
    }

    public async Task<Guid> LinkExerciseToDayAsync(Guid userId, LinkExerciseToDayRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(request);

        if (request.WeekId < 1 || request.WeekId > 4)
        {
            throw new ArgumentException("WeekId must be between 1 and 4.", nameof(request));
        }

        if (request.DayId < 1 || request.DayId > 7)
        {
            throw new ArgumentException("DayId must be between 1 and 7.", nameof(request));
        }

        if (request.ExerciseId == Guid.Empty)
        {
            throw new ArgumentException("ExerciseId is required.", nameof(request));
        }

        if (!await _repository.AllExercisesExistAsync([request.ExerciseId], cancellationToken))
        {
            throw new ArgumentException("Exercise is invalid or deleted.", nameof(request));
        }

        var monthlyPlan = await _repository.GetPlanSkeletonByUserIdAsync(userId, cancellationToken);
        if (monthlyPlan is null)
        {
            monthlyPlan = Domain.Entities.MonthlyPlan.Create(userId, activeDays: 7);
            await _repository.AddAsync(monthlyPlan, cancellationToken);
        }

        monthlyPlan.EnsureStructure();

        if (request.DayId > monthlyPlan.ActiveDays)
        {
            throw new ArgumentException("DayId must be within configured active days.", nameof(request));
        }

        await _repository.SaveChangesAsync(cancellationToken);

        // Link exercise to the same day (request.DayId) across all 4 weeks
        Guid firstLinkedId = Guid.Empty;

        for (int weekNumber = 1; weekNumber <= 4; weekNumber++)
        {
            var dayExercises = await _repository.GetDayExercisesAsync(monthlyPlan.Id, weekNumber, request.DayId, cancellationToken);

            if (dayExercises.Any(x => x.ExerciseId == request.ExerciseId))
            {
                // Exercise already linked to this week/day, skip
                continue;
            }

            var nextOrder = dayExercises.Count == 0
                ? 0
                : dayExercises.Max(x => x.OrderIndex) + 1;

            var linkedExercise = PlannedExercise.Create(
                monthlyPlan.Id,
                userId,
                weekNumber,
                request.DayId,
                request.ExerciseId,
                nextOrder);

            await _repository.AddPlannedExerciseAsync(linkedExercise, cancellationToken);

            // Store the first linked ID to return
            if (firstLinkedId == Guid.Empty)
            {
                firstLinkedId = linkedExercise.Id;
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return firstLinkedId;
    }

    public async Task UpdatePlannedExerciseAsync(Guid userId, Guid plannedExerciseId, UpdatePlannedExerciseRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(request);

        if (request.ExerciseId == Guid.Empty)
        {
            throw new ArgumentException("ExerciseId is required.", nameof(request));
        }

        var plannedExercise = await _repository.GetPlannedExerciseAsync(plannedExerciseId, cancellationToken);
        if (plannedExercise is null || plannedExercise.UserId != userId)
        {
            throw new KeyNotFoundException("Planned exercise not found.");
        }

        if (!await _repository.AllExercisesExistAsync([request.ExerciseId], cancellationToken))
        {
            throw new ArgumentException("Exercise is invalid or deleted.", nameof(request));
        }

        // Update exercise in all weeks with the same day number
        for (int weekNumber = 1; weekNumber <= 4; weekNumber++)
        {
            var dayExercises = await _repository.GetDayExercisesAsync(plannedExercise.MonthlyPlanId, weekNumber, plannedExercise.DayNumber, cancellationToken);

            // Find the exercise with the same exerciseId in this week's day and update it
            var exerciseInWeek = dayExercises.FirstOrDefault(x => x.ExerciseId == plannedExercise.ExerciseId);
            if (exerciseInWeek != null)
            {
                exerciseInWeek.UpdateExercise(request.ExerciseId);
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlinkExerciseAsync(Guid userId, Guid plannedExerciseId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        var plannedExercise = await _repository.GetPlannedExerciseAsync(plannedExerciseId, cancellationToken);
        if (plannedExercise is null || plannedExercise.UserId != userId)
        {
            throw new KeyNotFoundException("Planned exercise not found.");
        }

        // Get the day number and exercise info to unlink from all weeks
        var dayNumber = plannedExercise.DayNumber;
        var monthlyPlanId = plannedExercise.MonthlyPlanId;
        var exerciseId = plannedExercise.ExerciseId;

        // Remove exercise from all weeks with the same day number
        for (int weekNumber = 1; weekNumber <= 4; weekNumber++)
        {
            var dayExercises = await _repository.GetDayExercisesAsync(monthlyPlanId, weekNumber, dayNumber, cancellationToken);
            var exerciseToRemove = dayExercises.FirstOrDefault(x => x.ExerciseId == exerciseId);
            if (exerciseToRemove != null)
            {
                await _repository.RemovePlannedExerciseAsync(exerciseToRemove, cancellationToken);
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateUpsertRequest(UpsertMonthlyPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ActiveDays < 1 || request.ActiveDays > 7)
        {
            throw new ArgumentException("ActiveDays must be between 1 and 7.", nameof(request));
        }

        var dayKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var day in request.Days)
        {
            if (day.WeekNumber < 1 || day.WeekNumber > 4)
            {
                throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(request));
            }

            if (day.DayNumber < 1 || day.DayNumber > request.ActiveDays)
            {
                throw new ArgumentException("DayNumber must be within configured active days.", nameof(request));
            }

            var key = $"{day.WeekNumber}:{day.DayNumber}";
            if (!dayKeys.Add(key))
            {
                throw new ArgumentException("Duplicate week/day entries are not allowed.", nameof(request));
            }

            var exerciseKeys = new HashSet<Guid>();
            foreach (var exercise in day.Exercises)
            {
                if (exercise.ExerciseId == Guid.Empty)
                {
                    throw new ArgumentException("ExerciseId is required.", nameof(request));
                }

                if (exercise.OrderIndex < 0)
                {
                    throw new ArgumentException("OrderIndex must be greater than or equal to 0.", nameof(request));
                }

                if (!exerciseKeys.Add(exercise.ExerciseId))
                {
                    throw new ArgumentException("Duplicate ExerciseId inside the same day is not allowed.", nameof(request));
                }
            }
        }
    }

    private static void SaveAsLastConfirmed(Guid userId, MonthlyPlanResponse plan)
    {
        LastConfirmedPlans[userId] = new CachedMonthlyPlanState(plan, DateTimeOffset.UtcNow);
    }

    private async Task<MonthlyPlanResponse> MapAsync(Domain.Entities.MonthlyPlan monthlyPlan, CancellationToken cancellationToken)
    {
        var exerciseIds = monthlyPlan.PlannedExercises
            .Select(x => x.ExerciseId)
            .Distinct()
            .ToArray();

        var exerciseDetails = exerciseIds.Length > 0
            ? await _repository.GetExerciseDetailsAsync(exerciseIds, cancellationToken)
            : new Dictionary<Guid, ExerciseDetailDto>();

        var plannedLookup = monthlyPlan.PlannedExercises
            .GroupBy(x => new { x.WeekNumber, x.DayNumber })
            .ToDictionary(
                keySelector: x => $"{x.Key.WeekNumber}:{x.Key.DayNumber}",
                elementSelector: x => (IReadOnlyCollection<MonthlyPlanExerciseResponse>)x
                    .OrderBy(row => row.OrderIndex)
                    .Select(row =>
                    {
                        var detail = exerciseDetails.TryGetValue(row.ExerciseId, out var info) ? info : null;
                        return new MonthlyPlanExerciseResponse(
                            row.Id,
                            row.ExerciseId,
                            row.OrderIndex,
                            detail?.Name,
                            detail?.ExerciseType,
                            detail?.PrimaryMuscles);
                    })
                    .ToArray());

        var weeks = monthlyPlan.Weeks
            .OrderBy(x => x.WeekNumber)
            .Select(week => new MonthlyPlanWeekResponse(
                week.WeekNumber,
                week.Days
                    .OrderBy(day => day.DayNumber)
                    .Select(day => new MonthlyPlanDayResponse(
                        week.WeekNumber,
                        day.DayNumber,
                        day.Status.ToString(),
                        day.Status == PlanDayStatus.Truncated,
                        plannedLookup.TryGetValue($"{week.WeekNumber}:{day.DayNumber}", out var exercises)
                            ? exercises
                            : Array.Empty<MonthlyPlanExerciseResponse>()))
                    .ToArray()))
            .ToArray();

        return new MonthlyPlanResponse(
            monthlyPlan.Id,
            monthlyPlan.ActiveDays,
            weeks);
    }
}
