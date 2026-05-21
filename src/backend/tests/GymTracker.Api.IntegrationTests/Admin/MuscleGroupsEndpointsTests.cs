using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class MuscleGroupsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MuscleGroupsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateListAndDeleteMuscleGroup()
    {
        var createRequest = new
        {
            name = "Pecho",
            code = "CHEST",
            description = "Grupo muscular de pecho",
            active = true,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/muscle-groups", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        created.TryGetProperty("id", out var idProperty).Should().BeTrue();
        var id = idProperty.GetString();
        id.Should().NotBeNullOrWhiteSpace();

        var listResponse = await _client.GetAsync("/api/admin/muscle-groups?page=1&pageSize=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest = new
        {
            name = "Pecho Mayor",
            code = "CHEST_MAIN",
            description = "Grupo muscular de pecho actualizado",
            active = true,
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/muscle-groups/{id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/admin/muscle-groups/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
