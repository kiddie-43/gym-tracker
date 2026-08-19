using GymTracker.Domain.Entities;

namespace GymTracker.Application.Examples;

public interface IExampleRepository
{
    Task<IReadOnlyCollection<Example>> ListAsync(CancellationToken cancellationToken = default);

    Task<Example?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(Example entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
