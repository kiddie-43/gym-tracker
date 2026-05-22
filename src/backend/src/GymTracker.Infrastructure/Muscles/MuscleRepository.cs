using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;

using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.Muscles;

public sealed class MuscleRepository : IMuscleRepository
{
    private readonly AdminDbContext _context;

    public MuscleRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<Muscle?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Muscles
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Muscles.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(m => !m.IsDeleted);
        }

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, string? excludingId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Muscles
            .AnyAsync(m => !m.IsDeleted && m.Active
                && m.Code == code
                && (excludingId == null || m.Id != excludingId),
                cancellationToken);
    }

    public async Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Muscles.AnyAsync(m => m.Id == entity.Id, cancellationToken);

        if (exists)
        {
            _context.Muscles.Update(entity);
        }
        else
        {
            _context.Muscles.Add(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.SoftDelete(now);
        await _context.SaveChangesAsync(cancellationToken);
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
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
