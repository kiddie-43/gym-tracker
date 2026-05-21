using GymTracker.Application.Admin.Common;
using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Admin.ExerciseFormTypes;

public sealed class ExerciseFormTypeService
{
    private readonly IExerciseFormTypeRepository _repository;

    public ExerciseFormTypeService(IExerciseFormTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ExerciseFormTypeResponse>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(cancellationToken);
        var filtered = AdminSoftDeleteFilter.Apply(entities, item => item.IsDeleted, item => item.Active, includeInactive);
        return filtered.Select(Map).ToArray();
    }

    public async Task<ExerciseFormTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null || entity.IsDeleted ? null : Map(entity);
    }

    public async Task<ExerciseFormTypeResponse> CreateAsync(UpsertExerciseFormTypeRequest request, CancellationToken cancellationToken = default)
    {
        var existingCodes = (await _repository.ListAsync(cancellationToken))
            .Where(item => !item.IsDeleted)
            .Select(item => item.Code);

        AdminUniqueCodeValidator.EnsureUnique(request.Code, existingCodes, nameof(request.Code));

        var entity = ExerciseFormType.Create(request.Name, request.Code, request.Description, MapFields(request.Fields));
        if (!request.Active)
        {
            entity.Update(entity.Name, entity.Code, entity.Description, entity.Fields, active: false);
        }

        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<ExerciseFormTypeResponse?> UpdateAsync(string id, UpsertExerciseFormTypeRequest request, CancellationToken cancellationToken = default)
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

        entity.Update(request.Name, request.Code, request.Description, MapFields(request.Fields), request.Active);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task<ExerciseFormTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var existingCodes = (await _repository.ListAsync(cancellationToken))
            .Where(item => !item.IsDeleted && !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Code);

        AdminUniqueCodeValidator.EnsureUnique(entity.Code, existingCodes, nameof(entity.Code));

        await _repository.ReactivateAsync(id, cancellationToken);
        return Map(entity);
    }

    private static IReadOnlyCollection<ExerciseFormField> MapFields(IReadOnlyCollection<ExerciseFormFieldRequest> fields)
    {
        return fields
            .Select(field => ExerciseFormField.Create(
                field.Id,
                field.Name,
                field.Label,
                field.Type,
                field.Required,
                field.Unit,
                field.Min,
                field.Max,
                field.Options,
                field.SortOrder))
            .ToArray();
    }

    private static ExerciseFormTypeResponse Map(ExerciseFormType entity)
    {
        return new ExerciseFormTypeResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.Fields.Select(field => new ExerciseFormFieldResponse(
                field.Id,
                field.Name,
                field.Label,
                field.Type,
                field.Required,
                field.Unit,
                field.Min,
                field.Max,
                field.Options,
                field.SortOrder)).ToArray(),
            entity.Active,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt);
    }
}
