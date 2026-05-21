using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Routines;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Routines;

public sealed class RoutineSessionsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoutineSessionsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-sessions-v2");
    }

    [Fact]
    public async Task PostSessions_ShouldCreateAndRejectConflictingDays()
    {
        var createRoutineResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto("Pull", "Goal"));
        var routine = await createRoutineResponse.Content.ReadFromJsonAsync<RoutineDetailDto>();

        var createFirst = await _client.PostAsJsonAsync(
            $"/api/routines/{routine!.Id}/sessions",
            new CreateRoutineSessionDto("Dia A", new[] { "monday" }));

        createFirst.StatusCode.Should().Be(HttpStatusCode.Created);

        var createConflict = await _client.PostAsJsonAsync(
            $"/api/routines/{routine.Id}/sessions",
            new CreateRoutineSessionDto("Dia B", new[] { "monday" }));

        createConflict.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
