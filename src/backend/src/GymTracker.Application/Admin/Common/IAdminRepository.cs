namespace GymTracker.Application.Admin.Common;

public interface IAdminRepository<TEntity>
{
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(string id, TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
