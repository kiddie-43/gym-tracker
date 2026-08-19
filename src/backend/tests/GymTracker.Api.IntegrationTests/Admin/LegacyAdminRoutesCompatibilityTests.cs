using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class LegacyAdminRoutesCompatibilityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LegacyAdminRoutesCompatibilityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }

    [Theory]
    [InlineData("/api/admin/exercise-types")]
    [InlineData("/api/admin/exercise-form-types")]
    public async Task LegacyListRoutes_ShouldReturnNotFound(string legacyRoute)
    {
        var response = await _client.GetAsync(legacyRoute);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
