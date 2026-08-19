using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using FluentAssertions;

using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Application.Contracts.Training;
using GymTracker.Domain.Entities;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class TrainingMetricLogsCrudIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _ownerClient;
    private readonly HttpClient _otherUserClient;

    public TrainingMetricLogsCrudIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _ownerClient = factory.CreateClient();
        _ownerClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-training-metric-owner");

        _otherUserClient = factory.CreateClient();
        _otherUserClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-training-metric-other");
    }

    [Fact]
    public async Task CrudFlow_ShouldRespectOwnershipAndSoftDeleteByGroup()
    {
        var request = new CreateTrainingMetricGroupRequestDto(
            RoutineId: "routine-012",
            SessionId: "session-012",
            TrainingId: "training-012",
            ExerciseId: "exercise-012",
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            Metrics: new[]
            {
                new GroupMetricValueInputDto("metric-a", 100),
                new GroupMetricValueInputDto("metric-b", 200),
            });

        var createResponse = await _ownerClient.PostAsJsonAsync("/api/training-metric-logs", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdGroup = await createResponse.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        createdGroup.Should().NotBeNull();
        createdGroup!.GroupId.Should().NotBeNullOrWhiteSpace();
        createdGroup.Items.Should().HaveCount(2);
        createdGroup.Items.Select(item => item.GroupId).Distinct().Should().ContainSingle();

        var ownerGet = await _ownerClient.GetAsync($"/api/training-metric-logs/groups/{createdGroup.GroupId}");
        ownerGet.StatusCode.Should().Be(HttpStatusCode.OK);

        var otherUserGet = await _otherUserClient.GetAsync($"/api/training-metric-logs/groups/{createdGroup.GroupId}");
        otherUserGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var patchResponse = await _ownerClient.PatchAsJsonAsync(
            $"/api/training-metric-logs/groups/{createdGroup.GroupId}/metrics/metric-a",
            new UpdateMetricValueRequestDto(333));

        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var patchedLog = await patchResponse.Content.ReadFromJsonAsync<TrainingMetricLogDto>();
        patchedLog.Should().NotBeNull();
        patchedLog!.Value.Should().Be(333);

        var deleteResponse = await _ownerClient.DeleteAsync($"/api/training-metric-logs/groups/{createdGroup.GroupId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDelete = await _ownerClient.GetAsync($"/api/training-metric-logs/groups/{createdGroup.GroupId}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_ShouldApplyPaginationAndFilters()
    {
        var createResponse = await _ownerClient.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-list",
                SessionId: "session-list",
                TrainingId: "training-list",
                ExerciseId: "exercise-list",
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Metrics: new[]
                {
                    new GroupMetricValueInputDto("metric-list", 10),
                }));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await _ownerClient.GetAsync("/api/training-metric-logs?routineId=routine-list&page=0&pageSize=1");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResult = await listResponse.Content.ReadFromJsonAsync<TrainingMetricLogsPageResponseDto>();
        listResult.Should().NotBeNull();
        listResult!.Page.Should().Be(0);
        listResult.PageSize.Should().Be(1);
        listResult.Items.Count.Should().BeLessOrEqualTo(1);
        listResult.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PartialDeleteByMetricRoute_ShouldNotExist()
    {
        var response = await _ownerClient.DeleteAsync("/api/training-metric-logs/groups/group-abc/metrics/metric-a");

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task PatchValue_ShouldRejectImmutableFieldsInPayload()
    {
        var createResponse = await _ownerClient.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-immutable",
                SessionId: "session-immutable",
                TrainingId: "training-immutable",
                ExerciseId: "exercise-immutable",
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Metrics: new[]
                {
                    new GroupMetricValueInputDto("metric-immutable", 10),
                }));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        createdGroup.Should().NotBeNull();

        var payload = JsonSerializer.Serialize(new { value = 11, routineId = "tampered-routine" });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var patchResponse = await _ownerClient.PatchAsync(
            $"/api/training-metric-logs/groups/{createdGroup!.GroupId}/metrics/metric-immutable",
            content);

        patchResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PatchValue_ShouldRejectGroupIdMutationInPayload()
    {
        var createResponse = await _ownerClient.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-groupid",
                SessionId: "session-groupid",
                TrainingId: "training-groupid",
                ExerciseId: "exercise-groupid",
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Metrics: new[]
                {
                    new GroupMetricValueInputDto("metric-groupid", 42),
                }));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        createdGroup.Should().NotBeNull();

        var payload = JsonSerializer.Serialize(new { value = 43, groupId = "tampered-group" });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var patchResponse = await _ownerClient.PatchAsync(
            $"/api/training-metric-logs/groups/{createdGroup!.GroupId}/metrics/metric-groupid",
            content);

        patchResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MutableOperations_ShouldPreserveCurrentDayResolutionByLatestCreatedAt()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var firstCreate = await _ownerClient.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-resolution",
                SessionId: "session-resolution",
                TrainingId: "training-resolution",
                ExerciseId: "exercise-resolution",
                Date: today,
                Metrics: new[] { new GroupMetricValueInputDto("metric-resolution", 10) }));

        firstCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        var firstGroup = await firstCreate.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        firstGroup.Should().NotBeNull();

        await Task.Delay(50);

        var secondCreate = await _ownerClient.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-resolution",
                SessionId: "session-resolution",
                TrainingId: "training-resolution",
                ExerciseId: "exercise-resolution",
                Date: today,
                Metrics: new[] { new GroupMetricValueInputDto("metric-resolution", 20) }));

        secondCreate.StatusCode.Should().Be(HttpStatusCode.Created);
        var secondGroup = await secondCreate.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        secondGroup.Should().NotBeNull();

        var currentDayBeforeMutations = await _ownerClient.GetAsync(
            "/api/training-metric-logs/current-day?routineId=routine-resolution&sessionId=session-resolution&trainingId=training-resolution&exerciseId=exercise-resolution");
        currentDayBeforeMutations.StatusCode.Should().Be(HttpStatusCode.OK);
        var currentDayLogsBefore = await currentDayBeforeMutations.Content.ReadFromJsonAsync<IReadOnlyCollection<TrainingMetricLogDto>>();
        currentDayLogsBefore.Should().NotBeNull();
        currentDayLogsBefore!.Select(x => x.GroupId).Distinct().Single().Should().Be(secondGroup!.GroupId);

        var patchLatest = await _ownerClient.PatchAsJsonAsync(
            $"/api/training-metric-logs/groups/{secondGroup.GroupId}/metrics/metric-resolution",
            new UpdateMetricValueRequestDto(25));
        patchLatest.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteLatest = await _ownerClient.DeleteAsync($"/api/training-metric-logs/groups/{secondGroup.GroupId}");
        deleteLatest.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var currentDayAfterDelete = await _ownerClient.GetAsync(
            "/api/training-metric-logs/current-day?routineId=routine-resolution&sessionId=session-resolution&trainingId=training-resolution&exerciseId=exercise-resolution");
        currentDayAfterDelete.StatusCode.Should().Be(HttpStatusCode.OK);
        var currentDayLogsAfter = await currentDayAfterDelete.Content.ReadFromJsonAsync<IReadOnlyCollection<TrainingMetricLogDto>>();
        currentDayLogsAfter.Should().NotBeNull();
        var currentDayLogsAfterNotNull = currentDayLogsAfter!;
        currentDayLogsAfterNotNull.Should().NotBeEmpty();
        currentDayLogsAfterNotNull.Select(x => x.GroupId).Distinct().Single().Should().Be(firstGroup!.GroupId);
        currentDayLogsAfterNotNull.Select(x => x.Value).Should().OnlyContain(v => v == 10);
    }

    [Fact]
    public async Task LegacyEndpoint_ShouldNotExposeTrainingMetricLogsGroupSurface()
    {
        var legacyGroupGet = await _ownerClient.GetAsync("/api/exercise-training-logs/groups/legacy-group");
        legacyGroupGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var legacyMetricPatch = await _ownerClient.PatchAsJsonAsync(
            "/api/exercise-training-logs/groups/legacy-group/metrics/legacy-metric",
            new UpdateMetricValueRequestDto(11));
        legacyMetricPatch.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var legacyMetricDelete = await _ownerClient.DeleteAsync("/api/exercise-training-logs/groups/legacy-group");
        legacyMetricDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MetricsDegradation_ShouldKeepLogsIntegrity_WhenDefinitionsDependencyFails()
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
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-training-metric-owner");

        var createResponse = await client.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-degraded-integrity",
                SessionId: "session-degraded-integrity",
                TrainingId: "training-degraded-integrity",
                ExerciseId: "exercise-degraded-integrity",
                Date: DateOnly.FromDateTime(DateTime.UtcNow),
                Metrics: new[]
                {
                    new GroupMetricValueInputDto("metric-degraded-integrity", 15),
                }));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        createdGroup.Should().NotBeNull();

        var metricsResponse = await client.GetAsync("/api/training-metric-logs/metrics?exerciseId=exercise-degraded-integrity");
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        metricsResponse.Headers.TryGetValues("X-Metric-Definitions-Status", out var values).Should().BeTrue();
        values!.Should().ContainSingle().Which.Should().Be("degraded");

        var metricsPayload = await metricsResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<object>>();
        metricsPayload.Should().NotBeNull();
        metricsPayload!.Should().BeEmpty();

        var groupResponse = await client.GetAsync($"/api/training-metric-logs/groups/{createdGroup!.GroupId}");
        groupResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var groupPayload = await groupResponse.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();
        groupPayload.Should().NotBeNull();
        groupPayload!.Items.Should().NotBeEmpty();
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
