using GymTracker.Domain.Entities;

namespace GymTracker.Application.Examples;

public sealed class ExampleService
{
    private readonly IExampleRepository _repository;

    public ExampleService(IExampleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ExampleResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(cancellationToken);
        return entities
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(Map)
            .ToArray();
    }

    public async Task<ExampleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<ExampleResponse> CreateAsync(UpsertExampleRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(request.Code);

        if (await _repository.ExistsByCodeAsync(normalizedCode, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Code already exists.");
        }

        var entity = Example.Create(normalizedCode, request.Name, request.Description);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<ExampleResponse?> UpdateAsync(Guid id, UpsertExampleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var normalizedCode = NormalizeCode(request.Code);
        if (await _repository.ExistsByCodeAsync(normalizedCode, excludeId: id, cancellationToken))
        {
            throw new InvalidOperationException("Code already exists.");
        }

        entity.Update(normalizedCode, request.Name, request.Description);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    private static string NormalizeCode(string? code)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Code is required.", nameof(code));
        }

        return normalized;
    }

    private static ExampleResponse Map(Example entity)
    {
        return new ExampleResponse(entity.Id, entity.Code, entity.Name, entity.Description);
    }
}
