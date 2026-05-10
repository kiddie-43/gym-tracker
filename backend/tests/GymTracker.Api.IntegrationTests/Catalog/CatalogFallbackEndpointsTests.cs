using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Catalog;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Catalog;

public sealed class CatalogFallbackEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CatalogFallbackEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-catalog-fallback");
    }

    [Fact]
    public async Task GetStatus_ShouldReturnAvailabilityPayload()
    {
        var response = await _client.GetAsync("/api/catalog/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<CatalogAvailabilityDto>();
        payload.Should().NotBeNull();
    }
}
