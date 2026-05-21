using FluentAssertions;

using GymTracker.Application.Admin.MeasurementTypes;

namespace GymTracker.Application.UnitTests.Admin.MeasurementTypes;

public sealed class MeasurementTypeValidationTests
{
    private readonly MeasurementTypeService _service = new(new FakeMeasurementTypeRepository());

    [Theory]
    [InlineData("float")]
    [InlineData("number")]
    [InlineData("duration")]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenDataTypeIsInvalid(string dataType)
    {
        var request = CreateValidRequest() with { DataType = dataType };

        var action = () => _service.CreateAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*DataType is invalid*");
    }

    [Theory]
    [InlineData("power")]
    [InlineData("hypertrophy")]
    [InlineData("balance")]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenCategoryIsInvalid(string category)
    {
        var request = CreateValidRequest() with { Category = category };

        var action = () => _service.CreateAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Category is invalid*");
    }

    [Theory]
    [InlineData("integer", "strength")]
    [InlineData("decimal", "cardio")]
    [InlineData("time", "mobility")]
    [InlineData("boolean", "general")]
    [InlineData("text", "general")]
    public async Task CreateAsync_ShouldAcceptAllowedDataTypeAndCategory(string dataType, string category)
    {
        var request = CreateValidRequest() with { DataType = dataType, Category = category };

        var response = await _service.CreateAsync(request, CancellationToken.None);

        response.DataType.Should().Be(dataType);
        response.Category.Should().Be(category);
    }

    private static UpsertMeasurementTypeRequest CreateValidRequest()
    {
        return new UpsertMeasurementTypeRequest
        {
            Key = "WEIGHT",
            Name = "Peso",
            Unit = "kg",
            DataType = "decimal",
            Category = "strength",
            Description = "Carga",
            Active = true,
        };
    }

    private sealed class FakeMeasurementTypeRepository : IMeasurementTypeRepository
    {
        public Task<MeasurementTypesPageResponse> ListPageAsync(
            bool includeInactive = false,
            string? search = null,
            string? code = null,
            string sortBy = "name",
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
            return Task.FromResult(new MeasurementTypeResponse(
                Id: Guid.NewGuid().ToString("N"),
                Key: request.Key ?? string.Empty,
                Name: request.Name ?? string.Empty,
                Unit: request.Unit ?? string.Empty,
                DataType: request.DataType ?? string.Empty,
                Category: request.Category ?? string.Empty,
                Description: request.Description,
                Active: request.Active,
                IsDeleted: false,
                CreatedAt: DateTimeOffset.UtcNow,
                UpdatedAt: DateTimeOffset.UtcNow,
                DeletedAt: null));
        }

        public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementTypeResponse?>(null);
    }
}
