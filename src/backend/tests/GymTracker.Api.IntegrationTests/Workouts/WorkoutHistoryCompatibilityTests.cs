using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Workouts;

public sealed class WorkoutHistoryCompatibilityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkoutHistoryCompatibilityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-history");
    }

    [Fact]
    public async Task WorkoutHistory_ShouldRemainReadable_WhenExerciseIsNotAvailableInCatalog()
    {
        var createWorkoutResponse = await _client.PostAsJsonAsync("/api/workouts", new
        {
            performedAt = DateTimeOffset.UtcNow,
            status = "completed",
            routineId = (string?)null,
            notes = "snapshot",
            exerciseEntries = new[]
            {
                new
                {
                    externalExerciseId = "legacy-removed-exercise",
                    exerciseName = "Ejercicio legado",
                    muscleGroupIds = new[] { "legacy" },
                    sets = new[]
                    {
                        new { repetitions = 8, weight = 80, restSeconds = 120, completed = true },
                    },
                    notes = (string?)null,
                    imageUrl = (string?)null,
                },
            },
        });

        createWorkoutResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createWorkoutResponse.Content.ReadFromJsonAsync<JsonElement>();
        var workoutId = created!.GetProperty("id").GetString();

        var getByIdResponse = await _client.GetAsync($"/api/workouts/{workoutId}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
