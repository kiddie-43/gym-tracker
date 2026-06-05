using GymTracker.Application.Common.Importing;
using DomainUnit = GymTracker.Domain.Entities.Units;

namespace GymTracker.Application.Units;

public sealed class UnitsImportDefinition
    : ICsvImportDefinition<DomainUnit, UnitsResponse>
{
    private readonly IUnitsRepository _repository;

    public UnitsImportDefinition(IUnitsRepository repository)
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

    public DomainUnit CreateEntity(Dictionary<string, string?> row)
    {
        var code = row.GetValueOrDefault("code")!;
        var name = row.GetValueOrDefault("name") ?? code;
        var description = row.GetValueOrDefault("description") ?? string.Empty;

        return DomainUnit.Create(
            name: name,
            code: code,
            description: description);
    }

    public Task SaveAsync(
        DomainUnit entity,
        CancellationToken cancellationToken)
    {
        return _repository.SaveAsync(entity, cancellationToken);
    }

    public UnitsResponse Map(DomainUnit entity)
    {
        return new UnitsResponse(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description);
    }
}