using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Training;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Contracts;

public sealed class RoutinesTrainingContractsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoutinesTrainingContractsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-contracts-v2");
    }

    [Fact]
    public async Task CriticalContracts_ShouldReturnExpectedStatusCodes()
    {
        var routines = await _client.GetAsync("/api/routines");
        routines.StatusCode.Should().Be(HttpStatusCode.OK);

        var startFlow = await _client.PostAsJsonAsync("/api/training-flow/start", new StartTrainingFlowDto("routine-1"));
        startFlow.StatusCode.Should().Be(HttpStatusCode.OK);

        var missingComparison = await _client.GetAsync("/api/exercise-training-logs/progress-comparison?exerciseId=not-found");
        missingComparison.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
