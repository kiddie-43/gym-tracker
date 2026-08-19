using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Training;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class ExerciseTrainingLogsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExerciseTrainingLogsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-log-v2");
    }

    [Fact]
    public async Task PostAndGet_ShouldKeepOwnershipBoundToTokenUser()
    {
        var createdResponse = await _client.PostAsJsonAsync("/api/exercise-training-logs", new CreateExerciseTrainingLogDto(
            "routine-1",
            "session-1",
            "exercise-1",
            new[] { new PerformedSetDto(8, 20, 1) },
            "ok",
            new[] { new TrainingAttachmentDto("photo", "https://a") }));

        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createdResponse.Content.ReadFromJsonAsync<ExerciseTrainingLogDto>();

        var get = await _client.GetAsync($"/api/exercise-training-logs/{created!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
