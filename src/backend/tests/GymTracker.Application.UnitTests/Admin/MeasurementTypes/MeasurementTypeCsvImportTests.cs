using FluentAssertions;

using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.UnitTests.Admin.MeasurementTypes;

public sealed class MeasurementTypeCsvImportTests
{
    [Fact]
    public async Task ImportCsvAsync_ShouldProcessRowsPartially_AndReportRejectedRows()
    {
        var repository = new InMemoryMeasurementTypeRepository();
        var service = new MeasurementTypeService(repository);

        var result = await service.ImportCsvAsync(new ImportMeasurementTypesRequest(new[]
        {
            new ImportMeasurementTypeRowRequest("WEIGHT", "Peso", "Carga principal"),
            new ImportMeasurementTypeRowRequest("WEIGHT", "Peso duplicado", "Duplicada"),
            new ImportMeasurementTypeRowRequest(null, "Sin código", null),
        }));

        result.TotalRows.Should().Be(3);
        result.CreatedRows.Should().Be(1);
        result.RejectedRows.Should().Be(2);
        result.Rows.Count(row => row.Created).Should().Be(1);
        result.Rows.Count(row => !row.Created).Should().Be(2);
    }

    private sealed class InMemoryMeasurementTypeRepository : IMeasurementTypeRepository
    {
        private readonly Dictionary<string, MeasurementType> _store = new(StringComparer.OrdinalIgnoreCase);

        public Task<MeasurementType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.GetValueOrDefault(id));

        public Task<IReadOnlyCollection<MeasurementType>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var rows = includeDeleted
                ? _store.Values.ToArray()
                : _store.Values.Where(item => !item.IsDeleted).ToArray();

            return Task.FromResult<IReadOnlyCollection<MeasurementType>>(rows);
        }

        public Task<bool> ExistsActiveCodeAsync(string code, string? excludeId = null, CancellationToken cancellationToken = default)
        {
            var exists = _store.Values.Any(item =>
                !item.IsDeleted
                && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(item.Id, excludeId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }

        public Task SaveAsync(MeasurementType entity, CancellationToken cancellationToken = default)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            if (!_store.TryGetValue(id, out var entity))
            {
                return Task.FromResult(false);
            }

            entity.SoftDelete(now);
            return Task.FromResult(true);
        }

        public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!_store.TryGetValue(id, out var entity))
            {
                return Task.FromResult(false);
            }

            entity.Reactivate();
            return Task.FromResult(true);
        }
    }
}
