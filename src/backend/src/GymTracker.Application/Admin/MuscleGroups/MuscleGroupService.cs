using GymTracker.Application.Admin.Common;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.MuscleGroups;

public sealed class MuscleGroupService
{
    private readonly IMuscleGroupRepository _repository;

    public MuscleGroupService(IMuscleGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<MuscleGroupResponse>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(cancellationToken);
        var filtered = AdminSoftDeleteFilter.Apply(entities, item => item.IsDeleted, item => item.Active, includeInactive);
        return filtered.Select(Map).ToArray();
    }

    public async Task<MuscleGroupResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null || entity.IsDeleted ? null : Map(entity);
    }

    public async Task<MuscleGroupResponse> CreateAsync(UpsertMuscleGroupRequest request, CancellationToken cancellationToken = default)
    {
        var existingCodes = (await _repository.ListAsync(cancellationToken))
            .Where(item => !item.IsDeleted)
            .Select(item => item.Code);

        AdminUniqueCodeValidator.EnsureUnique(request.Code, existingCodes, nameof(request.Code));

        var entity = MuscleGroup.Create(request.Name, request.Code, request.Description);
        if (!request.Active)
        {
            entity.Update(entity.Name, entity.Code, entity.Description, active: false);
        }

        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<MuscleGroupResponse?> UpdateAsync(string id, UpsertMuscleGroupRequest request, CancellationToken cancellationToken = default)
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

        entity.Update(request.Name, request.Code, request.Description, request.Active);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);
    }

    private static MuscleGroupResponse Map(MuscleGroup entity)
    {
        return new MuscleGroupResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.Active,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt);
    }
}
