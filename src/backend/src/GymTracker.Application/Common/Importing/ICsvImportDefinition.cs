namespace GymTracker.Application.Common.Importing;

public interface ICsvImportDefinition<TEntity, TResponse>
{
    string CodeColumn { get; }

    Task<HashSet<string>> GetExistingCodesAsync(CancellationToken cancellationToken);

    TEntity CreateEntity(Dictionary<string, string?> row);

    Task SaveAsync(TEntity entity, CancellationToken cancellationToken);

    TResponse Map(TEntity entity);
}