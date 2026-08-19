namespace GymTracker.Application.Units;
using DomainUnit = GymTracker.Domain.Entities.Units;

public interface IUnitsRepository
{
    Task<IReadOnlyCollection<DomainUnit>> ListAsync(CancellationToken cancellationToken = default);

    Task<DomainUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(DomainUnit entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
