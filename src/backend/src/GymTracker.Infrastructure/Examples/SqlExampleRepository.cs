using GymTracker.Application.Examples;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;

using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.Examples;

public sealed class SqlExampleRepository : IExampleRepository
{
    private readonly AdminDbContext _context;

    public SqlExampleRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Example>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Examples
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Example?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Examples
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Examples
            .AnyAsync(item => item.Code == code && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);
    }

    public async Task SaveAsync(Example entity, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Examples.AnyAsync(item => item.Id == entity.Id, cancellationToken);

        if (exists)
        {
            _context.Examples.Update(entity);
        }
        else
        {
            _context.Examples.Add(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Examples.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _context.Examples.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}