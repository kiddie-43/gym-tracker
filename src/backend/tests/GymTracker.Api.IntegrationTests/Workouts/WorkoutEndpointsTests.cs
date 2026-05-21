using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Workouts;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Workouts;

public sealed class WorkoutEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkoutEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user");
    }

    [Fact]
    public async Task Post_ShouldCreateWorkout_WhenRequestIsValid()
    {
        var request = new CreateWorkoutRequest(
            DateTimeOffset.Parse("2026-05-05T10:00:00Z"),
            "completed",
            null,
            "Push day",
            new[]
            {
                new CreateExerciseEntryRequest(
                    "bench-press",
                    "Bench press",
                    new[] { "chest" },
                    new[] { new CreateWorkoutSetRequest(8, 100m, 120, true) },
                    null,
                    null),
            });

        var response = await _client.PostAsJsonAsync("/api/workouts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<WorkoutResponse>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("completed");
        payload.Id.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetById_ShouldReturnWorkoutWithExerciseEntriesAndSets()
    {
        var request = new CreateWorkoutRequest(
            DateTimeOffset.Parse("2026-05-09T13:00:00Z"),
            "completed",
            null,
            "Leg day",
            new[]
            {
                new CreateExerciseEntryRequest(
                    "squat",
                    "Sentadilla",
                    new[] { "legs" },
                    new[]
                    {
                        new CreateWorkoutSetRequest(8, 100m, 120, true),
                        new CreateWorkoutSetRequest(6, 110m, 150, true),
                    },
                    "Serie pesada",
                    null),
            });

        var createResponse = await _client.PostAsJsonAsync("/api/workouts", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdWorkout = await createResponse.Content.ReadFromJsonAsync<WorkoutResponse>();

        var response = await _client.GetAsync($"/api/workouts/{createdWorkout!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<WorkoutResponse>();

        payload.Should().NotBeNull();
        payload!.ExerciseEntries.Should().NotBeNull();
        payload.ExerciseEntries!.Should().HaveCount(1);
        var entry = payload.ExerciseEntries!.First();
        entry.ExternalExerciseId.Should().Be("squat");
        entry.ExerciseNameSnapshot.Should().Be("Sentadilla");
        entry.Sets.Should().HaveCount(2);
        entry.Sets.First().Repetitions.Should().Be(8);
        entry.Sets.First().Weight.Should().Be(100m);
    }
}
