using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Workouts;

public sealed class WorkoutCatalogEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly HttpClient _adminClient;

    public WorkoutCatalogEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-catalog");

        _adminClient = factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Get_ShouldReturnAdminExerciseCatalog()
    {
        await _adminClient.PostAsJsonAsync("/api/admin/exercises", new
        {
            name = "Sentadilla",
            code = $"SQUAT_{Guid.NewGuid():N}"[..12],
            description = "Piernas",
            exerciseTypeId = "type-strength",
            exerciseTypeCode = "STRENGTH",
            formTypeId = "form-strength",
            formTypeCode = "STRENGTH_BASIC",
            primaryMuscleIds = new[] { "legs" },
            secondaryMuscleIds = new[] { "glutes" },
            muscleGroupIds = new[] { "lower-body" },
            active = true,
        });

        var response = await _client.GetAsync("/api/workouts/exercise-catalog?query=sentad");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        payload.Should().NotBeNull();
        payload!.Length.Should().BeGreaterThan(0);
    }
}
