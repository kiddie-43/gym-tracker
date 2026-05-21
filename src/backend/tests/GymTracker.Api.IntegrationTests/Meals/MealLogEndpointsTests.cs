using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Meals;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Meals;

public sealed class MealLogEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MealLogEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-meals");
    }

    [Fact]
    public async Task PostAndGet_ShouldRoundtripMealLogs()
    {
        var request = new CreateMealLogRequest(
            DateOnly.Parse("2026-05-05"),
            "breakfast",
            new[]
            {
                new MealItemInput("oats", 80, "g", 300),
            });

        var post = await _client.PostAsJsonAsync("/api/meals/logs", request);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var get = await _client.GetAsync("/api/meals/logs?date=2026-05-05");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await get.Content.ReadFromJsonAsync<IReadOnlyCollection<MealLogResponse>>();
        payload.Should().HaveCount(1);
        payload!.Single().SlotType.Should().Be("breakfast");
    }
}
