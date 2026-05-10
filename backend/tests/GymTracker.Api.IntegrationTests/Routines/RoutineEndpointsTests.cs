using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Routines;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Routines;

public sealed class RoutineEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoutineEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-routines");
    }

    [Fact]
    public async Task Crud_ShouldCreateUpdateListAndDeleteRoutine()
    {
        var createRequest = CreateRequest("Push split", "Monday");
        var createResponse = await _client.PostAsJsonAsync("/api/routines", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<RoutineResponse>();
        created.Should().NotBeNull();

        var listResponse = await _client.GetAsync("/api/routines");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<RoutineSummaryResponse>>();
        list.Should().ContainSingle(item => item.Id == created!.Id);

        var updateResponse = await _client.PutAsJsonAsync($"/api/routines/{created!.Id}", CreateRequest("Push split updated", "Tuesday"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<RoutineResponse>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Push split updated");
        updated.Days.Single().DayLabel.Should().Be("Tuesday");

        var getByIdResponse = await _client.GetAsync($"/api/routines/{created.Id}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/routines/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await _client.GetAsync($"/api/routines/{created.Id}");
        afterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static CreateRoutineRequest CreateRequest(string name, string dayLabel)
    {
        return new CreateRoutineRequest(
            name,
            "Upper body",
            new[] { "push" },
            new[]
            {
                new CreateRoutineDayRequest(
                    dayLabel,
                    new[]
                    {
                        new CreatePlannedExerciseRequest(
                            "bench-press",
                            "Bench press",
                            new[] { "chest" },
                            4,
                            8,
                            120,
                            null,
                            null),
                    }),
            });
    }
}
