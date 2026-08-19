using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class AdminEndpointsTestFixture : IClassFixture<WebApplicationFactory<Program>>
{
    public HttpClient Client { get; }

    public AdminEndpointsTestFixture(WebApplicationFactory<Program> factory)
    {
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "integration-admin-user");
    }
}
