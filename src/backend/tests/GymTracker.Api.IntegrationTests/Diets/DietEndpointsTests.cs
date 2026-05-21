using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Diets;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Diets;

public sealed class DietEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DietEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-diets");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateListAndDeleteDiet()
    {
        var createRequest = CreateRequest("Lean week");
        var createResponse = await _client.PostAsJsonAsync("/api/diets", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<DietResponse>();
        created.Should().NotBeNull();

        var listResponse = await _client.GetAsync("/api/diets");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<DietSummaryResponse>>();
        list.Should().ContainSingle(item => item.Id == created!.Id);

        var updateResponse = await _client.PutAsJsonAsync($"/api/diets/{created!.Id}", CreateRequest("Lean week v2"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/diets/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static CreateDietRequest CreateRequest(string name)
    {
        return new CreateDietRequest(
            name,
            new[]
            {
                new DietDayInput(
                    "monday",
                    new[]
                    {
                        new MealSlotInput(
                            "breakfast",
                            new[] { new MealItemInput("oats", 80, "g", 300) }),
                    }),
            });
    }
}
