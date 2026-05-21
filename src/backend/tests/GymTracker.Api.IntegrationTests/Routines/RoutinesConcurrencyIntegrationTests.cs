using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Routines;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Routines;

public sealed class RoutinesConcurrencyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoutinesConcurrencyIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-concurrency-v2");
    }

    [Fact]
    public async Task UpdateRoutine_ShouldFollowLastWriteWinsPolicy()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto("Base", "Goal"));
        var created = await createResponse.Content.ReadFromJsonAsync<RoutineDetailDto>();

        var firstWrite = await _client.PatchAsJsonAsync($"/api/routines/{created!.Id}", new UpdateRoutineDto("Version A", "Goal A"));
        firstWrite.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondWrite = await _client.PatchAsJsonAsync($"/api/routines/{created.Id}", new UpdateRoutineDto("Version B", "Goal B"));
        secondWrite.StatusCode.Should().Be(HttpStatusCode.OK);

        var current = await _client.GetFromJsonAsync<RoutineDetailDto>($"/api/routines/{created.Id}");
        current!.Title.Should().Be("Version B");
        current.Goal.Should().Be("Goal B");
    }
}
