using System.Collections.Concurrent;

using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.ExerciseTypes;

public sealed class ExerciseTypeRepository : IExerciseTypeRepository
{
    private static readonly ConcurrentDictionary<string, ExerciseType> Store = new(StringComparer.OrdinalIgnoreCase);

    public Task<ExerciseType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<ExerciseType>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<ExerciseType>>(Store.Values.ToArray());
    }

    public Task SaveAsync(ExerciseType entity, CancellationToken cancellationToken = default)
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
}
