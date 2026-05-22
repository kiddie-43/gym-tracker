using GymTracker.Application.Admin.Exercises;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Admin;

using Microsoft.EntityFrameworkCore;

namespace GymTracker.Infrastructure.Exercises;

public sealed class SqlExerciseRepository : IExerciseRepository
{
    private readonly AdminDbContext _context;

    public SqlExerciseRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
            return null;

        await LoadRelationsAsync([entity], cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyCollection<Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Exercises.AsQueryable();
        if (!includeDeleted)
            query = query.Where(e => !e.IsDeleted);

        var rows = await query.ToArrayAsync(cancellationToken);
        await LoadRelationsAsync(rows, cancellationToken);
        return rows;
    }

    public async Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.Exercises.AnyAsync(
            e => !e.IsDeleted && e.Active && e.Code == normalized && e.Id != excludeExerciseId,
            cancellationToken);
    }

    public async Task SaveAsync(Exercise entity, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Exercises.AnyAsync(e => e.Id == entity.Id, cancellationToken);

        if (exists)
        {
            _context.Exercises.Update(entity);
        }
        else
        {
            _context.Exercises.Add(entity);
        }

        await SyncRelationsAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null || entity.IsDeleted) return false;
        entity.SoftDelete(now);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null || !entity.IsDeleted) return false;
        entity.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task LoadRelationsAsync(IReadOnlyCollection<Exercise> exercises, CancellationToken cancellationToken)
    {
        if (exercises.Count == 0)
            return;

        var exerciseIds = exercises.Select(e => e.Id).ToArray();

        var primaryByExercise = (await _context.ExercisePrimaryMuscles
                .Where(x => exerciseIds.Contains(x.ExerciseId))
                .ToArrayAsync(cancellationToken))
            .GroupBy(x => x.ExerciseId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.OrderBy(x => x.SortOrder).Select(x => x.MuscleId).ToArray(), StringComparer.OrdinalIgnoreCase);

        var secondaryByExercise = (await _context.ExerciseSecondaryMuscles
                .Where(x => exerciseIds.Contains(x.ExerciseId))
                .ToArrayAsync(cancellationToken))
            .GroupBy(x => x.ExerciseId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.OrderBy(x => x.SortOrder).Select(x => x.MuscleId).ToArray(), StringComparer.OrdinalIgnoreCase);

        var measurementByExercise = (await _context.ExerciseMeasurementTypes
                .Where(x => exerciseIds.Contains(x.ExerciseId))
                .ToArrayAsync(cancellationToken))
            .GroupBy(x => x.ExerciseId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.OrderBy(x => x.SortOrder).Select(x => x.MeasurementTypeId).ToArray(), StringComparer.OrdinalIgnoreCase);

        foreach (var exercise in exercises)
        {
            var primary = primaryByExercise.GetValueOrDefault(exercise.Id, Array.Empty<string>());
            var secondary = secondaryByExercise.GetValueOrDefault(exercise.Id, Array.Empty<string>());
            var measurement = measurementByExercise.GetValueOrDefault(exercise.Id, Array.Empty<string>());
            exercise.LoadRelations(primary, secondary, measurement);
        }
    }

    private async Task SyncRelationsAsync(Exercise entity, CancellationToken cancellationToken)
    {
        await SyncPrimaryMusclesAsync(entity.Id, entity.PrimaryMuscleIds, cancellationToken);
        await SyncSecondaryMusclesAsync(entity.Id, entity.SecondaryMuscleIds, cancellationToken);
        await SyncMeasurementTypesAsync(entity.Id, entity.MeasurementTypeIds, cancellationToken);
    }

    private async Task SyncPrimaryMusclesAsync(string exerciseId, IReadOnlyCollection<string> targetIds, CancellationToken cancellationToken)
    {
        var existing = await _context.ExercisePrimaryMuscles
            .Where(x => x.ExerciseId == exerciseId)
            .ToArrayAsync(cancellationToken);
        if (existing.Length > 0)
            _context.ExercisePrimaryMuscles.RemoveRange(existing);

        var toAdd = targetIds
            .Select((id, idx) => new ExercisePrimaryMuscle
            {
                ExerciseId = exerciseId,
                MuscleId = id,
                SortOrder = idx,
            })
            .ToArray();

        if (toAdd.Length > 0)
            await _context.ExercisePrimaryMuscles.AddRangeAsync(toAdd, cancellationToken);
    }

    private async Task SyncSecondaryMusclesAsync(string exerciseId, IReadOnlyCollection<string> targetIds, CancellationToken cancellationToken)
    {
        var existing = await _context.ExerciseSecondaryMuscles
            .Where(x => x.ExerciseId == exerciseId)
            .ToArrayAsync(cancellationToken);
        if (existing.Length > 0)
            _context.ExerciseSecondaryMuscles.RemoveRange(existing);

        var toAdd = targetIds
            .Select((id, idx) => new ExerciseSecondaryMuscle
            {
                ExerciseId = exerciseId,
                MuscleId = id,
                SortOrder = idx,
            })
            .ToArray();

        if (toAdd.Length > 0)
            await _context.ExerciseSecondaryMuscles.AddRangeAsync(toAdd, cancellationToken);
    }

    private async Task SyncMeasurementTypesAsync(string exerciseId, IReadOnlyCollection<string> targetIds, CancellationToken cancellationToken)
    {
        var existing = await _context.ExerciseMeasurementTypes
            .Where(x => x.ExerciseId == exerciseId)
            .ToArrayAsync(cancellationToken);
        if (existing.Length > 0)
            _context.ExerciseMeasurementTypes.RemoveRange(existing);

        var toAdd = targetIds
            .Select((id, idx) => new ExerciseMeasurementType
            {
                ExerciseId = exerciseId,
                MeasurementTypeId = id,
                SortOrder = idx,
            })
            .ToArray();

        if (toAdd.Length > 0)
            await _context.ExerciseMeasurementTypes.AddRangeAsync(toAdd, cancellationToken);
    }
}
