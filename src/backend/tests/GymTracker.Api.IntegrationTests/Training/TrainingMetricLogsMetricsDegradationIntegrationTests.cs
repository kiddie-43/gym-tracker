using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class TrainingMetricLogsMetricsDegradationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TrainingMetricLogsMetricsDegradationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Metrics_ShouldReturnEmptyWithoutDegradedHeader_WhenExerciseDoesNotExist()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "integration-training-metric-owner");

        var response = await client.GetAsync("/api/training-metric-logs/metrics?exerciseId=missing-exercise");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("X-Metric-Definitions-Status").Should().BeFalse();

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<object>>();
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    [Fact]
    public async Task Metrics_ShouldReturnDegradedHeader_WhenDefinitionsDependencyFails()
    {
        using var degradedFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IExerciseRepository, ThrowingExerciseRepository>();
                services.AddScoped<IMuscleRepository, EmptyMuscleRepository>();
                services.AddScoped<IMeasurementTypeRepository, EmptyMeasurementTypeRepository>();
            });
        });

        using var client = degradedFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "integration-training-metric-owner");

        var response = await client.GetAsync("/api/training-metric-logs/metrics?exerciseId=exercise-any");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Metric-Definitions-Status", out var values).Should().BeTrue();
        values!.Should().ContainSingle().Which.Should().Be("degraded");

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<object>>();
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    private sealed class ThrowingExerciseRepository : IExerciseRepository
    {
        public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated dependency failure.");

        public Task<IReadOnlyCollection<Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Exercise>>([]);

        public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task SaveAsync(Exercise entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class EmptyMuscleRepository : IMuscleRepository
    {
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<Muscle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Muscle?>(null);

        public Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Muscle>>([]);

        public Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class EmptyMeasurementTypeRepository : IMeasurementTypeRepository
    {
        public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsActiveCodeAsync(string code, string? excludeId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<MeasurementType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<MeasurementType?>(null);

        public Task<IReadOnlyCollection<MeasurementType>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<MeasurementType>>([]);

        public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task SaveAsync(MeasurementType entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}