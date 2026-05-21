using System.Collections.Concurrent;

using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.MuscleGroups;

public sealed class MuscleGroupRepository : IMuscleGroupRepository
{
    private static readonly ConcurrentDictionary<string, MuscleGroup> Store = new(StringComparer.OrdinalIgnoreCase);

    public Task<MuscleGroup?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<MuscleGroup>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<MuscleGroup>>(Store.Values.ToArray());
    }

    public Task SaveAsync(MuscleGroup entity, CancellationToken cancellationToken = default)
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
