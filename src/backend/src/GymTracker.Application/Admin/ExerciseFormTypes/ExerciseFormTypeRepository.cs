using System.Collections.Concurrent;

using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.ExerciseFormTypes;

public sealed class ExerciseFormTypeRepository : IExerciseFormTypeRepository
{
    private static readonly ConcurrentDictionary<string, ExerciseFormType> Store = new(StringComparer.OrdinalIgnoreCase);

    public Task<ExerciseFormType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<ExerciseFormType>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<ExerciseFormType>>(Store.Values.ToArray());
    }

    public Task SaveAsync(ExerciseFormType entity, CancellationToken cancellationToken = default)
    {
        Store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.SoftDelete(now);
        Store[id] = existing;
        return true;
    }

    public async Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Reactivate();
        Store[id] = existing;
        return true;
    }
}
