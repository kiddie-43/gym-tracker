using GymTracker.Application.Common.Importing;
using GymTracker.Domain.Entities;

public sealed class ExerciceImportDefinition : ICsvImportDefinition<Exercice, ExerciceResponse>
{
    private readonly IExerciceRepository _repository;

    public ExerciceImportDefinition(IExerciceRepository repository)
    {
        _repository = repository;
    }

    public string CodeColumn => "code";

    public async Task<HashSet<string>> GetExistingCodesAsync(CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(cancellationToken);

        return items
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public Exercice CreateEntity(Dictionary<string, string?> row)
    {
        var code = row.GetValueOrDefault("code")!;
        var name = row.GetValueOrDefault("name") ?? code;
        var description = row.GetValueOrDefault("description");

        return Exercice.Create(
            name: name,
            code: code,
            description: description);
    }

    public Task SaveAsync(Exercice entity, CancellationToken cancellationToken)
    {
        return _repository.SaveAsync(entity, cancellationToken);
    }

    public ExerciceResponse Map(Exercice entity)
    {
        return new ExerciceResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            [],
            [],
            []);
    }
}
