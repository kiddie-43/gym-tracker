using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;

using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.MeasurementTypes;

public sealed class SqlMeasurementTypeRepository : IMeasurementTypeRepository
{
    private readonly AdminDbContext _context;

    public SqlMeasurementTypeRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<MeasurementType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _context.MeasurementTypes.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<MeasurementType>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.MeasurementTypes.AsQueryable();
        if (!includeDeleted)
            query = query.Where(e => !e.IsDeleted);
        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.MeasurementTypes.AnyAsync(
            e => !e.IsDeleted && e.Active && e.Code == normalized && e.Id != excludeId,
            cancellationToken);
    }

    public async Task SaveAsync(MeasurementType entity, CancellationToken cancellationToken = default)
    {
        var exists = await _context.MeasurementTypes.AnyAsync(e => e.Id == entity.Id, cancellationToken);
        if (exists) _context.MeasurementTypes.Update(entity);
        else _context.MeasurementTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MeasurementTypes.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null || entity.IsDeleted) return false;
        entity.SoftDelete(now);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MeasurementTypes.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null || !entity.IsDeleted) return false;
        entity.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
