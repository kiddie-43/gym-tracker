using System.Collections.Concurrent;

using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Muscles;

public sealed class MuscleRepository : IMuscleRepository
{
    private static readonly ConcurrentDictionary<string, Muscle> Store = new(StringComparer.OrdinalIgnoreCase);

    public Task<Muscle?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var values = includeDeleted
            ? Store.Values.ToArray()
            : Store.Values.Where(item => !item.IsDeleted).ToArray();

        return Task.FromResult<IReadOnlyCollection<Muscle>>(values);
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, string? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var rows = await ListAsync(includeDeleted: false, cancellationToken);
        return rows.Any(item =>
            string.Equals(item.Code, normalizedCode, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(item.Id, excludingId, StringComparison.OrdinalIgnoreCase));
    }

    public Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
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
