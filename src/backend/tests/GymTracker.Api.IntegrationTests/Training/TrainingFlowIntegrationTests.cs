using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Contracts.Training;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Training;

public sealed class TrainingFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TrainingFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-flow-v2");
    }

    [Fact]
    public async Task StartGetAndCancel_ShouldRoundtripFlowState()
    {
        var start = await _client.PostAsJsonAsync("/api/training-flow/start", new StartTrainingFlowDto("routine-1"));
        start.StatusCode.Should().Be(HttpStatusCode.OK);

        var active = await _client.GetAsync("/api/training-flow/active");
        active.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancel = await _client.PostAsync("/api/training-flow/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
