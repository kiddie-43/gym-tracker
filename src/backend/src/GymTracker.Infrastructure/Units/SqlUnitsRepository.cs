using GymTracker.Application.Units;
using GymTracker.Infrastructure.Admin;
using DomainUnit = GymTracker.Domain.Entities.Units;

using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.Units;

public sealed class SqlUnitsRepository : IUnitsRepository
{
    private readonly AdminDbContext _context;

    public SqlUnitsRepository(AdminDbContext context)
    {
        _context = context;
    }
    public async Task<IReadOnlyCollection<DomainUnit>> ListAsync( CancellationToken cancellationToken = default)
    {
        var query = _context.Units.AsNoTracking().AsQueryable();

  

        return (await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken)).Where(unit => unit.DeletedAt == null).ToArray();
    }

    public async Task<DomainUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Units
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Units
            .AnyAsync(x =>
                !x.DeletedAt.HasValue &&
                x.Code == normalizedCode &&
                (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken);
    }

    public async Task SaveAsync(DomainUnit entity, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            var exists = await _context.Units
                .AnyAsync(x => x.Id == entity.Id, cancellationToken);

            if (exists)
                _context.Units.Update(entity);
            else
                await _context.Units.AddAsync(entity, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Units
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null || entity.DeletedAt.HasValue)
            return false;

        entity.Delete(null);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Units
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null || !entity.DeletedAt.HasValue)
        {
            return false;
        }

        entity.Restore();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
