using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;
using Microsoft.EntityFrameworkCore;

public sealed class ExerciceRepository : IExerciceRepository
{
    private readonly AdminDbContext _context;

    public ExerciceRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<Exercice> CreateAsync(CreateExerciceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = Exercice.Create(request.Name, request.Code, request.Description);

        await AttachRelationsAsync(entity, request.Units, request.PrimaryMuscles, request.SecondaryMuscles, cancellationToken);

        await _context.Exercices.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<Exercice?> UpdateAsync(Guid id, UpdateExerciceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _context.Exercices
            .Include(x => x.Units)
                .ThenInclude(x => x.Unit)
            .Include(x => x.Muscles)
                .ThenInclude(x => x.Muscle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Update(request.Name, request.Description);
        await SyncRelationsAsync(entity, request.Units, request.PrimaryMuscles, request.SecondaryMuscles, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<bool> UnitsExistAsync(IReadOnlyCollection<Guid> unitIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(unitIds);

        if (unitIds.Any(id => id == Guid.Empty))
        {
            return false;
        }

        var distinctIds = unitIds
            .Distinct()
            .ToArray();

        if (distinctIds.Length == 0)
        {
            return true;
        }

        var count = await _context.Units
            .CountAsync(x => distinctIds.Contains(x.Id) && x.DeletedAt == null, cancellationToken);

        return count == distinctIds.Length;
    }

    public async Task<bool> MusclesExistAsync(IReadOnlyCollection<Guid> muscleIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(muscleIds);

        if (muscleIds.Any(id => id == Guid.Empty))
        {
            return false;
        }

        var distinctIds = muscleIds
            .Distinct()
            .ToArray();

        if (distinctIds.Length == 0)
        {
            return true;
        }

        var count = await _context.Muscles
            .CountAsync(x => distinctIds.Contains(x.Id) && x.DeletedAt == null, cancellationToken);

        return count == distinctIds.Length;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Exercices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || entity.IsDeleted)
        {
            return false;
        }

        entity.Delete(null);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Exercices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || !entity.IsDeleted)
        {
            return false;
        }

        entity.Restore();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Exercices
            .AnyAsync(x => x.DeletedAt == null
                && x.Code == normalizedCode
                && (excludingId == null || x.Id != excludingId.Value),
                cancellationToken);
    }

    public async Task SaveAsync(Exercice entity, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            var exists = await _context.Exercices.AnyAsync(x => x.Id == entity.Id, cancellationToken);

            if (exists)
            {
                _context.Exercices.Update(entity);
            }
            else
            {
                await _context.Exercices.AddAsync(entity, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Exercice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Exercices
            .Include(x => x.Units)
                .ThenInclude(x => x.Unit)
            .Include(x => x.Muscles)
                .ThenInclude(x => x.Muscle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Exercice>> ListAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Exercices
            .Include(x => x.Units)
                .ThenInclude(x => x.Unit)
            .Include(x => x.Muscles)
                .ThenInclude(x => x.Muscle)
            .AsQueryable();

        return (await query.OrderBy(x => x.Name).ToListAsync(cancellationToken))
            .Where(x => x.DeletedAt == null)
            .ToArray();
    }

    private async Task AttachRelationsAsync(
        Exercice entity,
        IReadOnlyCollection<Guid> unitIds,
        IReadOnlyCollection<Guid> primaryMuscleIds,
        IReadOnlyCollection<Guid> secondaryMuscleIds,
        CancellationToken cancellationToken)
    {
        var distinctUnitIds = unitIds.Distinct().ToArray();
        var distinctPrimaryMuscleIds = primaryMuscleIds.Distinct().ToArray();
        var distinctSecondaryMuscleIds = secondaryMuscleIds.Distinct().ToArray();

        if (distinctUnitIds.Length > 0)
        {
            var units = await _context.Units
                .Where(x => distinctUnitIds.Contains(x.Id) && x.DeletedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var unit in units)
            {
                entity.AddUnit(unit);
            }
        }

        if (distinctPrimaryMuscleIds.Length > 0)
        {
            var primaryMuscles = await _context.Muscles
                .Where(x => distinctPrimaryMuscleIds.Contains(x.Id) && x.DeletedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var muscle in primaryMuscles)
            {
                entity.AddPrimaryMuscle(muscle);
            }
        }

        if (distinctSecondaryMuscleIds.Length > 0)
        {
            var secondaryMuscles = await _context.Muscles
                .Where(x => distinctSecondaryMuscleIds.Contains(x.Id) && x.DeletedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var muscle in secondaryMuscles)
            {
                entity.AddSecondaryMuscle(muscle);
            }
        }
    }

    private async Task SyncRelationsAsync(
        Exercice entity,
        IReadOnlyCollection<Guid> unitIds,
        IReadOnlyCollection<Guid> primaryMuscleIds,
        IReadOnlyCollection<Guid> secondaryMuscleIds,
        CancellationToken cancellationToken)
    {
        var targetUnitIds = unitIds.Distinct().ToHashSet();
        var targetPrimaryIds = primaryMuscleIds.Distinct().ToHashSet();
        var targetSecondaryIds = secondaryMuscleIds.Distinct().ToHashSet();

        foreach (var relation in entity.Units)
        {
            var shouldBeActive = targetUnitIds.Contains(relation.UnitId);

            if (shouldBeActive && relation.IsDeleted)
            {
                relation.Restore();
            }
            else if (!shouldBeActive && !relation.IsDeleted)
            {
                relation.Delete(null);
            }
        }

        foreach (var relation in entity.Muscles)
        {
            var shouldBeActive = relation.Type switch
            {
                ExerciceMuscleType.Primary => targetPrimaryIds.Contains(relation.MuscleId),
                ExerciceMuscleType.Secondary => targetSecondaryIds.Contains(relation.MuscleId),
                _ => false,
            };

            if (shouldBeActive && relation.IsDeleted)
            {
                relation.Restore();
            }
            else if (!shouldBeActive && !relation.IsDeleted)
            {
                relation.Delete(null);
            }
        }

        var existingUnitIds = entity.Units
            .Select(x => x.UnitId)
            .ToHashSet();

        var missingUnitIds = targetUnitIds
            .Where(id => !existingUnitIds.Contains(id))
            .ToArray();

        if (missingUnitIds.Length > 0)
        {
            var units = await _context.Units
                .Where(x => missingUnitIds.Contains(x.Id) && x.DeletedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var unit in units)
            {
                entity.AddUnit(unit);
            }
        }

        var existingMuscleKeys = entity.Muscles
            .Select(x => (x.MuscleId, x.Type))
            .ToHashSet();

        var missingPrimaryIds = targetPrimaryIds
            .Where(id => !existingMuscleKeys.Contains((id, ExerciceMuscleType.Primary)))
            .ToArray();

        var missingSecondaryIds = targetSecondaryIds
            .Where(id => !existingMuscleKeys.Contains((id, ExerciceMuscleType.Secondary)))
            .ToArray();

        var missingMuscleIds = missingPrimaryIds
            .Concat(missingSecondaryIds)
            .Distinct()
            .ToArray();

        if (missingMuscleIds.Length > 0)
        {
            var muscles = await _context.Muscles
                .Where(x => missingMuscleIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            foreach (var muscleId in missingPrimaryIds)
            {
                if (muscles.TryGetValue(muscleId, out var muscle))
                {
                    entity.AddPrimaryMuscle(muscle);
                }
            }

            foreach (var muscleId in missingSecondaryIds)
            {
                if (muscles.TryGetValue(muscleId, out var muscle))
                {
                    entity.AddSecondaryMuscle(muscle);
                }
            }
        }
    }
}
