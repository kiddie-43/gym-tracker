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

    public async Task<Muscle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Muscles
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Muscle>> ListAsync( CancellationToken cancellationToken = default)
    {
        var query = _context.Muscles.AsQueryable();
        return (await query.ToListAsync(cancellationToken)).Where(m => m.DeletedAt == null).ToArray();
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Muscles
            .AnyAsync(m => m.DeletedAt == null
                && m.Code == normalizedCode
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Delete(id);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null || !entity.IsDeleted)
        {
            return false;
        }

        entity.Restore();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
