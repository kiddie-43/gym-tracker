using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Domain.Entities;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

internal static class MeasurementTypesTestHost
{
    public static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        var testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMeasurementTypeRepository>();
                services.AddSingleton<IMeasurementTypeRepository, InMemoryMeasurementTypeRepository>();
            });
        });

        return testFactory.CreateClient();
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

            var hasConflict = _store.Values.Any(item =>
                !item.IsDeleted
                && !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.Code, entity.Code, StringComparison.OrdinalIgnoreCase));

            if (hasConflict)
            {
                return Task.FromResult(false);
            }

            entity.Reactivate();
            return Task.FromResult(true);
        }
    }
}
