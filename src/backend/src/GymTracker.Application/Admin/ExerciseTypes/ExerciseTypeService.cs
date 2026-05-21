using GymTracker.Application.Admin.Common;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.ExerciseTypes;

public sealed class ExerciseTypeService
{
    private readonly IExerciseTypeRepository _repository;

    public ExerciseTypeService(IExerciseTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ExerciseTypeResponse>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(cancellationToken);
        var filtered = AdminSoftDeleteFilter.Apply(entities, item => item.IsDeleted, item => item.Active, includeInactive);
        return filtered.Select(Map).ToArray();
    }

    public async Task<ExerciseTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null || entity.IsDeleted ? null : Map(entity);
    }

    public async Task<ExerciseTypeResponse> CreateAsync(UpsertExerciseTypeRequest request, CancellationToken cancellationToken = default)
    {
        var existingCodes = (await _repository.ListAsync(cancellationToken))
            .Where(item => !item.IsDeleted)
            .Select(item => item.Code);

        AdminUniqueCodeValidator.EnsureUnique(request.Code, existingCodes, nameof(request.Code));

        var entity = ExerciseType.Create(request.Name, request.Code, request.Description, request.PrimaryUnit, request.RequiresUnits);
        if (!request.Active)
        {
            entity.Update(entity.Name, entity.Code, entity.Description, entity.PrimaryUnit, entity.RequiresUnits, active: false);
        }

        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<ExerciseTypeResponse?> UpdateAsync(string id, UpsertExerciseTypeRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted)
        {
            return null;
        }

        var existingCodes = (await _repository.ListAsync(cancellationToken))
            .Where(item => !item.IsDeleted && !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Code);

        AdminUniqueCodeValidator.EnsureUnique(request.Code, existingCodes, nameof(request.Code));

        entity.Update(request.Name, request.Code, request.Description, request.PrimaryUnit, request.RequiresUnits, request.Active);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);
    }

    private static ExerciseTypeResponse Map(ExerciseType entity)
    {
        return new ExerciseTypeResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.PrimaryUnit,
            entity.RequiresUnits,
            entity.Active,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt);
    }
}
