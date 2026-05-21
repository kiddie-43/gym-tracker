using System.Net;
using System.Net.Http.Headers;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin.MeasurementTypes;

public sealed class MeasurementTypesAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementTypesAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _client = MeasurementTypesTestHost.CreateClient(factory);
    }

    [Fact]
    public async Task List_ShouldReturn401_WhenMissingAuthorizationHeader()
    {
        var response = await _client.GetAsync("/api/admin/measurement-types");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_ShouldReturn403_WhenUserIsAuthenticatedButNotAdmin()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "integration-user");

        var response = await _client.GetAsync("/api/admin/measurement-types");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task List_ShouldReturn200_WhenUserIsAdmin()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "integration-admin-user");

        var response = await _client.GetAsync("/api/admin/measurement-types");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
