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

    private sealed record SessionsListResponse(RoutineSessionDto[] Items, int Total);

    [Fact]
    public async Task PostSessions_ShouldCreateAndRejectConflictingDays()
    {
        var createRoutineResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto { Name = "Pull", Goal = "Goal" });
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

    [Fact]
    public async Task GetSessions_ShouldReturnSessionsForRoutine()
    {
        var createRoutineResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto { Name = "Push", Goal = "Goal" });
        var routine = await createRoutineResponse.Content.ReadFromJsonAsync<RoutineDetailDto>();

        var createSessionResponse = await _client.PostAsJsonAsync(
            $"/api/routines/{routine!.Id}/sessions",
            new CreateRoutineSessionDto("Dia A", new[] { "wednesday" }));

        createSessionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getSessionsResponse = await _client.GetAsync($"/api/routines/{routine.Id}/sessions");
        getSessionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessionsPage = await getSessionsResponse.Content.ReadFromJsonAsync<SessionsListResponse>();
        sessionsPage.Should().NotBeNull();
        if (sessionsPage is null)
        {
            throw new InvalidOperationException("Expected sessions response body.");
        }

        sessionsPage.Total.Should().Be(1);
        sessionsPage.Items.Should().ContainSingle();
        sessionsPage.Items[0].Name.Should().Be("Dia A");
    }
}
