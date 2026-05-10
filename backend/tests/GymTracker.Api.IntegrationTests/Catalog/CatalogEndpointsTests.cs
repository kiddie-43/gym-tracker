using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Catalog;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Catalog;

public sealed class CatalogEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CatalogEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-catalog");
    }

    [Fact]
    public async Task GetMuscleGroups_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/catalog/muscle-groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MuscleGroupDto>>();
        payload.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExercises_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/catalog/exercises?query=press&muscleGroupIds=chest");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ExerciseDto>>();
        payload.Should().NotBeNull();
    }
}
