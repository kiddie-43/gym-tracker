using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Training;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class ProgressComparisonIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProgressComparisonIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-progress-v2");
    }

    [Fact]
    public async Task GetProgressComparison_ShouldReturnComputedPayload_WhenLogsExist()
    {
        await _client.PostAsJsonAsync("/api/exercise-training-logs", new CreateExerciseTrainingLogDto(
            "routine-1",
            "session-1",
            "exercise-1",
            new[] { new PerformedSetDto(8, 20, 1) },
            "set-1",
            null));

        await _client.PostAsJsonAsync("/api/exercise-training-logs", new CreateExerciseTrainingLogDto(
            "routine-1",
            "session-1",
            "exercise-1",
            new[] { new PerformedSetDto(8, 25, 1) },
            "set-2",
            null));

        var response = await _client.GetAsync("/api/exercise-training-logs/progress-comparison?exerciseId=exercise-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ProgressComparisonDto>();
        payload.Should().NotBeNull();
        payload!.ExerciseId.Should().Be("exercise-1");
        payload.Baselines.Should().HaveCount(3);
    }
}
