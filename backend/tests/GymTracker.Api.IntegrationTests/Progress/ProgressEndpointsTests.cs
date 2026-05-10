using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Progress;
using GymTracker.Application.Workouts;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Progress;

public sealed class ProgressEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProgressEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-progress");
    }

    [Fact]
    public async Task GetByExercise_ShouldReturnProgressSnapshot_WhenEquivalentHistoryExists()
    {
        await _client.PostAsJsonAsync("/api/workouts", CreateRequest("2026-05-01T10:00:00Z", 90m));
        await _client.PostAsJsonAsync("/api/workouts", CreateRequest("2026-05-05T10:00:00Z", 100m));

        var response = await _client.GetAsync("/api/progress/exercises/bench-press");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ProgressResponse>();
        payload.Should().NotBeNull();
        payload!.ExerciseId.Should().Be("bench-press");
        payload.LastSessionComparison.Should().NotBeNull();
        payload.Trend.Should().Be("Improving");
        payload.Current.Volume.Should().Be(800m);
    }

    private static CreateWorkoutRequest CreateRequest(string performedAt, decimal weight)
    {
        return new CreateWorkoutRequest(
            DateTimeOffset.Parse(performedAt),
            "completed",
            null,
            null,
            new[]
            {
                new CreateExerciseEntryRequest(
                    "bench-press",
                    "Bench press",
                    new[] { "chest" },
                    new[] { new CreateWorkoutSetRequest(8, weight, 120, true) },
                    null,
                    null),
            });
    }
}
