using FluentAssertions;

using GymTracker.Application.Admin.MeasurementTypes;

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
        private readonly Dictionary<string, MeasurementTypeResponse> _store = new(StringComparer.OrdinalIgnoreCase);

        public Task<MeasurementTypesPageResponse> ListPageAsync(
            bool includeInactive = false,
            string? search = null,
            string? code = null,
            string sortBy = "code",
            string sortDirection = "asc",
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var rows = includeInactive
                ? _store.Values.ToArray()
                : _store.Values.Where(item => !item.IsDeleted).ToArray();

            return Task.FromResult(new MeasurementTypesPageResponse(
                rows,
                rows.Length,
                page,
                pageSize));
        }

        public Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
        {
            var rows = _store.Values
                .Where(item => !item.IsDeleted)
                .Select(item => new AssignableMeasurementTypeResponse(item.Id, item.Code, item.Name, item.Description))
                .ToArray();
            return Task.FromResult<IReadOnlyCollection<AssignableMeasurementTypeResponse>>(rows);
        }

        public Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var row);
            return Task.FromResult<MeasurementTypeResponse?>(row);
        }

        public Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
        {
            var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code is required.", nameof(request.Code));
            }

            var hasConflict = _store.Values.Any(item => !item.IsDeleted && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
            if (hasConflict)
            {
                throw new InvalidOperationException("Code already exists among active records.");
            }

            var name = string.IsNullOrWhiteSpace(request.Name) ? code : request.Name.Trim();

            var row = new MeasurementTypeResponse(
                Id: Guid.NewGuid().ToString("N"),
                Code: code,
                Name: name,
                Description: request.Description,
                IsDeleted: false);

            _store[row.Id] = row;
            return Task.FromResult(row);
        }

        public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);
    }
}
