using GymTracker.Domain.Entities;

public interface IExerciceRepository
{
    Task<Exercice> CreateAsync(CreateExerciceRequest request, CancellationToken cancellationToken = default);

    Task<Exercice?> UpdateAsync(Guid id, UpdateExerciceRequest request, CancellationToken cancellationToken = default);

    Task<bool> UnitsExistAsync(IReadOnlyCollection<Guid> unitIds, CancellationToken cancellationToken = default);

    Task<bool> MusclesExistAsync(IReadOnlyCollection<Guid> muscleIds, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(Exercice entity, CancellationToken cancellationToken = default);

    Task<Exercice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Exercice>> ListAsync(CancellationToken cancellationToken = default);
}