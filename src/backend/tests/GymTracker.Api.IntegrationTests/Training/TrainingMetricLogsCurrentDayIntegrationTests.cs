using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Training;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class TrainingMetricLogsCurrentDayIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TrainingMetricLogsCurrentDayIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-training-metric-current-day");
    }

    [Fact]
    public async Task CurrentDay_ShouldReturnEmptyArray_WhenNoLogsExistForContext()
    {
        var response = await _client.GetAsync(
            "/api/training-metric-logs/current-day?routineId=missing-routine&sessionId=missing-session&trainingId=missing-training&exerciseId=missing-exercise");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TrainingMetricLogDto>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CurrentDay_ShouldReturnLatestGroupByCreatedAt()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var firstGroupResponse = await _client.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-current",
                SessionId: "session-current",
                TrainingId: "training-current",
                ExerciseId: "exercise-current",
                Date: today,
                Metrics: new[] { new GroupMetricValueInputDto("metric-a", 1) }));

        firstGroupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await Task.Delay(50);

        var secondGroupResponse = await _client.PostAsJsonAsync(
            "/api/training-metric-logs",
            new CreateTrainingMetricGroupRequestDto(
                RoutineId: "routine-current",
                SessionId: "session-current",
                TrainingId: "training-current",
                ExerciseId: "exercise-current",
                Date: today,
                Metrics: new[] { new GroupMetricValueInputDto("metric-a", 2) }));

        secondGroupResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var secondGroup = await secondGroupResponse.Content.ReadFromJsonAsync<TrainingMetricGroupResponseDto>();

        var currentDayResponse = await _client.GetAsync(
            "/api/training-metric-logs/current-day?routineId=routine-current&sessionId=session-current&trainingId=training-current&exerciseId=exercise-current");

        currentDayResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var currentDayLogs = await currentDayResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<TrainingMetricLogDto>>();

        currentDayLogs.Should().NotBeNull();
        currentDayLogs.Should().NotBeEmpty();
        var groupIds = currentDayLogs!.Select(log => log.GroupId).Distinct().ToArray();
        groupIds.Should().ContainSingle();
        groupIds.Single().Should().Be(secondGroup!.GroupId);
    }

    [Fact]
    public async Task CurrentDay_ShouldRejectRequest_WhenExerciseIdIsMissing()
    {
        var response = await _client.GetAsync(
            "/api/training-metric-logs/current-day?routineId=routine-current&sessionId=session-current&trainingId=training-current");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
