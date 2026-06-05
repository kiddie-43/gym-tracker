using GymTracker.Application.Common.Importing;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Muscles;

public sealed class MuscleImportDefinition
    : ICsvImportDefinition<Muscle, MuscleResponse>
{
    private readonly IMuscleRepository _repository;

    public MuscleImportDefinition(IMuscleRepository repository)
    {
        _repository = repository;
    }

    public string CodeColumn => "code";

    public async Task<HashSet<string>> GetExistingCodesAsync(
        CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(
            cancellationToken);

        return items
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public Muscle CreateEntity(Dictionary<string, string?> row)
    {
        var code = row.GetValueOrDefault("code")!;
        var name = row.GetValueOrDefault("name") ?? code;
        var description = row.GetValueOrDefault("description") ?? string.Empty;

        return Muscle.Create(
            name: name,
            code: code,
            description: description);
    }

    public Task SaveAsync(
        Muscle entity,
        CancellationToken cancellationToken)
    {
        return _repository.SaveAsync(entity, cancellationToken);
    }

    public MuscleResponse Map(Muscle entity)
    {
        return new MuscleResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description);
    }
}