using System.Net;

using FluentAssertions;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Application.Contracts.Training;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Entities.Training;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.Api.IntegrationTests.Security;

public sealed class DevelopmentAuthModeSecurityTests
{
    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task ProtectedEndpoints_ShouldRejectRequestsWithoutAuthorization_WhenDevelopmentAuthModeIsEnabledOutsideDevelopment(string environmentName)
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environmentName);
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DevelopmentAuthMode:Enabled"] = "true",
                    ["DevelopmentAuthMode:UserId"] = "forbidden-dev-auth-user",
                    ["DevelopmentAuthMode:IsAdmin"] = "true",
                });
            });
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ITrainingMetricLogRepository, StubTrainingMetricLogRepository>();
                services.AddScoped<IExerciseRepository, StubExerciseRepository>();
                services.AddScoped<IMuscleRepository, StubMuscleRepository>();
                services.AddScoped<IMeasurementTypeRepository, StubMeasurementTypeRepository>();
            });
        });

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/training-metric-logs/current-day?routineId=security-routine&sessionId=security-session&trainingId=security-training");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed class StubTrainingMetricLogRepository : ITrainingMetricLogRepository
    {
        public Task<TrainingMetricGroupResponseDto> CreateGroupAsync(string userId, CreateTrainingMetricGroupRequestDto request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TrainingMetricGroupResponseDto?> GetGroupAsync(string userId, string groupId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyCollection<TrainingMetricLog>> GetCurrentDayAsync(string userId, string routineId, string sessionId, string trainingId, string? exerciseId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TrainingMetricLogsPageResponseDto> ListAsync(string userId, TrainingMetricLogsQueryDto query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> SoftDeleteGroupAsync(string userId, string groupId, string actionBy, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TrainingMetricLog?> UpdateMetricValueAsync(string userId, string groupId, string metricId, decimal value, string actionBy, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class StubExerciseRepository : IExerciseRepository
    {
        public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyCollection<Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveAsync(Exercise entity, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class StubMuscleRepository : IMuscleRepository
    {
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Muscle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class StubMeasurementTypeRepository : IMeasurementTypeRepository
    {
        public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsActiveCodeAsync(string code, string? excludeId = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<MeasurementType?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyCollection<MeasurementType>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SaveAsync(MeasurementType entity, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}