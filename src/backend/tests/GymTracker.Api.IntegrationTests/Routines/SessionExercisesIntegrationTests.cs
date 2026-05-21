using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Routines;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Routines;

public sealed class SessionExercisesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SessionExercisesIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-session-exercises-v2");
    }

    [Fact]
    public async Task AddAndUnlinkExercise_ShouldReturnExpectedStatusCodes()
    {
        var createRoutineResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto("Legs", "Goal"));
        var routine = await createRoutineResponse.Content.ReadFromJsonAsync<RoutineDetailDto>();

        var createSessionResponse = await _client.PostAsJsonAsync(
            $"/api/routines/{routine!.Id}/sessions",
            new CreateRoutineSessionDto("Dia A", new[] { "tuesday" }));
        var session = await createSessionResponse.Content.ReadFromJsonAsync<RoutineSessionDto>();

        var addExerciseResponse = await _client.PostAsJsonAsync(
            $"/api/routines/{routine.Id}/sessions/{session!.Id}/exercises",
            new AddSessionExerciseDto("exercise-1", "Sentadilla"));

        addExerciseResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var exercise = await addExerciseResponse.Content.ReadFromJsonAsync<SessionExerciseDto>();

        var unlinkResponse = await _client.DeleteAsync(
            $"/api/routines/{routine.Id}/sessions/{session.Id}/exercises/{exercise!.Id}");

        unlinkResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
