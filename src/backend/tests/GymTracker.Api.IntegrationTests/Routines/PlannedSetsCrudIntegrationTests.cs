using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Routines;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Routines;

public sealed class PlannedSetsCrudIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PlannedSetsCrudIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-sets-v2");
    }

    [Fact]
    public async Task PatchAndDeletePlannedSet_ShouldWorkForExistingSet()
    {
        var createdRoutineResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto("Push", "Goal"));
        var routine = await createdRoutineResponse.Content.ReadFromJsonAsync<RoutineDetailDto>();

        var createdSessionResponse = await _client.PostAsJsonAsync($"/api/routines/{routine!.Id}/sessions", new CreateRoutineSessionDto("A", new[] { "monday" }));
        var session = await createdSessionResponse.Content.ReadFromJsonAsync<RoutineSessionDto>();

        var createdExerciseResponse = await _client.PostAsJsonAsync(
            $"/api/routines/{routine.Id}/sessions/{session!.Id}/exercises",
            new AddSessionExerciseDto("exercise-1", "Press banca"));
        var exercise = await createdExerciseResponse.Content.ReadFromJsonAsync<SessionExerciseDto>();

        var setId = exercise!.PlannedSets.First().Id;

        var patch = await _client.PatchAsJsonAsync(
            $"/api/routines/{routine.Id}/sessions/{session.Id}/exercises/{exercise.Id}/planned-sets/{setId}",
            new UpdatePlannedSetDto(10, 30));

        patch.StatusCode.Should().Be(HttpStatusCode.OK);

        var delete = await _client.DeleteAsync(
            $"/api/routines/{routine.Id}/sessions/{session.Id}/exercises/{exercise.Id}/planned-sets/{setId}");

        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
