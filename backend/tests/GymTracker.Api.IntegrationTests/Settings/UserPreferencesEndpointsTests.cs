using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using GymTracker.Application.Settings;

using Microsoft.AspNetCore.Mvc.Testing;

namespace GymTracker.Api.IntegrationTests.Settings;

public sealed class UserPreferencesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserPreferencesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "integration-user-settings");
    }

    [Fact]
    public async Task PutAndGet_ShouldPersistPreferences()
    {
        var update = await _client.PutAsJsonAsync("/api/settings/preferences", new UpdatePreferencesRequest(true, 2200));
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await _client.GetAsync("/api/settings/preferences");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await get.Content.ReadFromJsonAsync<PreferencesResponse>();
        payload.Should().NotBeNull();
        payload!.CalorieTrackingEnabled.Should().BeTrue();
        payload.DailyCalorieGoal.Should().Be(2200);
    }
}
