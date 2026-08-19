using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Routines;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Routines;

public sealed class RoutinesCrudIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoutinesCrudIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-routines-v2");
    }

    [Fact]
    public async Task CrudAndReactivate_ShouldRespectSoftDelete()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/routines", new CreateRoutineDto { Name = "Push", Goal = "Goal" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<RoutineDetailDto>();
        created.Should().NotBeNull();

        var patchResponse = await _client.PatchAsJsonAsync($"/api/routines/{created!.Id}", new UpdateRoutineDto { Name = "Push 2", Goal = "Goal 2" });
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/routines/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reactivateResponse = await _client.PostAsync($"/api/routines/{created.Id}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
