using GymTracker.Application.MonthlyPlan;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.MonthlyPlan;

public sealed class MonthlyPlanRepository : IMonthlyPlanRepository
{
    private readonly AdminDbContext _dbContext;

    public MonthlyPlanRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Entities.MonthlyPlan?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MonthlyPlans
            .Include(x => x.Weeks)
                .ThenInclude(week => week.Days)
            .Include(x => x.PlannedExercises.Where(pe => pe.DeletedAt == null))
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null, cancellationToken);
    }

    public async Task<Domain.Entities.MonthlyPlan?> GetPlanSkeletonByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MonthlyPlans
            .Include(x => x.Weeks)
                .ThenInclude(week => week.Days)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<PlannedExercise>> GetDayExercisesAsync(Guid monthlyPlanId, int weekNumber, int dayNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlannedExercises
            .Where(x => x.MonthlyPlanId == monthlyPlanId && x.WeekNumber == weekNumber && x.DayNumber == dayNumber && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlannedExercise?> GetPlannedExerciseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlannedExercises
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    public async Task AddPlannedExerciseAsync(PlannedExercise exercise, CancellationToken cancellationToken = default)
    {
        await _dbContext.PlannedExercises.AddAsync(exercise, cancellationToken);
    }

    public Task RemovePlannedExerciseAsync(PlannedExercise exercise, CancellationToken cancellationToken = default)
    {
        _dbContext.PlannedExercises.Remove(exercise);
        return Task.CompletedTask;
    }

    public async Task AddAsync(Domain.Entities.MonthlyPlan monthlyPlan, CancellationToken cancellationToken = default)
    {
        await _dbContext.MonthlyPlans.AddAsync(monthlyPlan, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.MonthlyPlan monthlyPlan, CancellationToken cancellationToken = default)
    {
        // MonthlyPlan changes are persisted through the tracked aggregate loaded in this request.
        // Avoid DbSet.Update graph-wide state changes because child entities use assigned keys,
        // and forcing Modified can produce false optimistic concurrency exceptions.
        _ = monthlyPlan;

        return Task.CompletedTask;
    }

    public async Task AddHistoricalRecordsAsync(IReadOnlyCollection<HistoricalExerciseRecord> records, CancellationToken cancellationToken = default)
    {
        await _dbContext.HistoricalExerciseRecords.AddRangeAsync(records, cancellationToken);
    }

    public async Task<bool> AllExercisesExistAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken cancellationToken = default)
    {
        if (exerciseIds.Count == 0)
        {
            return true;
        }

        var uniqueIds = exerciseIds.Distinct().ToArray();
        var activeCount = await _dbContext.Exercices
            .Where(x => uniqueIds.Contains(x.Id) && x.DeletedAt == null)
            .CountAsync(cancellationToken);

        return activeCount == uniqueIds.Length;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetExerciseNamesAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken cancellationToken = default)
    {
        if (exerciseIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var uniqueIds = exerciseIds.Distinct().ToArray();
        return await _dbContext.Exercices
            .Where(x => uniqueIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, ExerciseDetailDto>> GetExerciseDetailsAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken cancellationToken = default)
    {
        if (exerciseIds.Count == 0)
        {
            return new Dictionary<Guid, ExerciseDetailDto>();
        }

        var uniqueIds = exerciseIds.Distinct().ToArray();
        var exercises = await _dbContext.Exercices
            .Where(x => uniqueIds.Contains(x.Id))
            .Include(x => x.Muscles.Where(m => m.Type == ExerciceMuscleType.Primary))
                .ThenInclude(m => m.Muscle)
            .ToListAsync(cancellationToken);

        return exercises.ToDictionary(
            x => x.Id,
            x => new ExerciseDetailDto(
                x.Name,
                x.ExerciseType.ToString(),
                x.Muscles
                    .Where(m => m.Type == ExerciceMuscleType.Primary)
                    .Select(m => m.Muscle.Name)
                    .ToArray() as IReadOnlyCollection<string>));
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
