using System.Collections.Concurrent;

using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Exercises;

public sealed class ExerciseRepository : IExerciseRepository
{
    private static readonly ConcurrentDictionary<string, Exercise> Store = new(StringComparer.OrdinalIgnoreCase);

    public Task<Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var rows = includeDeleted
            ? Store.Values
            : Store.Values.Where(item => !item.IsDeleted);

        return Task.FromResult<IReadOnlyCollection<Exercise>>(rows.ToArray());
    }

    public Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default)
    {
        var exists = Store.Values
            .Where(item => !item.IsDeleted && item.Active)
            .Where(item => string.IsNullOrWhiteSpace(excludeExerciseId)
                || !string.Equals(item.Id, excludeExerciseId, StringComparison.OrdinalIgnoreCase))
            .Any(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task SaveAsync(Exercise entity, CancellationToken cancellationToken = default)
    {
        Store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.SoftDelete(now);
        Store[id] = entity;
        return true;
    }

    public async Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Reactivate();
        Store[id] = entity;
        return true;
    }
}
