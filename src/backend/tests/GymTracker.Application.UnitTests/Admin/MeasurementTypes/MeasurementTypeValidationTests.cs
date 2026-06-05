using FluentAssertions;

using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Domain.Entities;

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
