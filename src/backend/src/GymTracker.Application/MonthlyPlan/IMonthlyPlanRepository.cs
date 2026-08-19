using GymTracker.Domain.Entities;

namespace GymTracker.Application.MonthlyPlan;

public record ExerciseDetailDto(string Name, string ExerciseType, IReadOnlyCollection<string> PrimaryMuscles);

public interface IMonthlyPlanRepository
{
    Task<Domain.Entities.MonthlyPlan?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Domain.Entities.MonthlyPlan?> GetPlanSkeletonByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Entities.MonthlyPlan monthlyPlan, CancellationToken cancellationToken = default);

    Task UpdateAsync(Domain.Entities.MonthlyPlan monthlyPlan, CancellationToken cancellationToken = default);

    Task AddHistoricalRecordsAsync(IReadOnlyCollection<HistoricalExerciseRecord> records, CancellationToken cancellationToken = default);

    Task<bool> AllExercisesExistAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string>> GetExerciseNamesAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ExerciseDetailDto>> GetExerciseDetailsAsync(IReadOnlyCollection<Guid> exerciseIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlannedExercise>> GetDayExercisesAsync(Guid monthlyPlanId, int weekNumber, int dayNumber, CancellationToken cancellationToken = default);

    Task<PlannedExercise?> GetPlannedExerciseAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddPlannedExerciseAsync(PlannedExercise exercise, CancellationToken cancellationToken = default);

    Task RemovePlannedExerciseAsync(PlannedExercise exercise, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

