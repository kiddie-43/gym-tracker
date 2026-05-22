using FluentAssertions;

using GymTracker.Application.Admin.MeasurementTypes;

namespace GymTracker.Application.UnitTests.Admin.MeasurementTypes;

public sealed class MeasurementTypeValidationTests
{
    private readonly MeasurementTypeService _service = new(new FakeMeasurementTypeRepository());

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenCodeIsEmpty(string? code)
    {
        var request = new UpsertMeasurementTypeRequest { Code = code, Name = "Test" };

        var action = () => _service.CreateAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldDefaultNameToCode_WhenNameIsEmpty()
    {
        var request = new UpsertMeasurementTypeRequest { Code = "WEIGHT", Name = null };

        var response = await _service.CreateAsync(request, CancellationToken.None);

        response.Name.Should().Be("WEIGHT");
    }

    [Fact]
    public async Task CreateAsync_ShouldUseProvidedName_WhenNameIsSet()
    {
        var request = new UpsertMeasurementTypeRequest { Code = "WEIGHT", Name = "Peso", Description = "Carga" };

        var response = await _service.CreateAsync(request, CancellationToken.None);

        response.Code.Should().Be("WEIGHT");
        response.Name.Should().Be("Peso");
        response.Description.Should().Be("Carga");
    }

    private sealed class FakeMeasurementTypeRepository : IMeasurementTypeRepository
    {
        public Task<MeasurementTypesPageResponse> ListPageAsync(
            bool includeInactive = false,
            string? search = null,
            string? code = null,
            string sortBy = "code",
            string sortDirection = "asc",
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new MeasurementTypesPageResponse(Array.Empty<MeasurementTypeResponse>(), 0, page, pageSize));

        public Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<AssignableMeasurementTypeResponse>>(Array.Empty<AssignableMeasurementTypeResponse>());

        public Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);

        public Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
        {
            var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code is required.", nameof(request.Code));
            }

            var name = string.IsNullOrWhiteSpace(request.Name) ? code : request.Name.Trim();

            return Task.FromResult(new MeasurementTypeResponse(
                Id: Guid.NewGuid().ToString("N"),
                Code: code,
                Name: name,
                Description: request.Description,
                IsDeleted: false));
        }

        public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);
    }
}
