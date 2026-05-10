using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Meals;
using GymTracker.Application.Workouts;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.History;

public sealed class HistoryEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HistoryEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-history");
    }

    [Fact]
    public async Task GetWorkoutHistory_ShouldReturnPagedItems()
    {
        await _client.PostAsJsonAsync("/api/workouts", new CreateWorkoutRequest(
            DateTimeOffset.Parse("2026-05-05T10:00:00Z"),
            "completed",
            null,
            null,
            new[] { new CreateExerciseEntryRequest("bench-press", "Bench press", new[] { "chest" }, new[] { new CreateWorkoutSetRequest(8, 100, 120, true) }, null, null) }));

        var response = await _client.GetAsync("/api/workouts?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMealHistory_ShouldReturnPagedItems()
    {
        await _client.PostAsJsonAsync("/api/meals/logs", new CreateMealLogRequest(
            DateOnly.Parse("2026-05-05"),
            "breakfast",
            new[] { new MealItemInput("oats", 80, "g", 300) }));

        var response = await _client.GetAsync("/api/meals/history?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
