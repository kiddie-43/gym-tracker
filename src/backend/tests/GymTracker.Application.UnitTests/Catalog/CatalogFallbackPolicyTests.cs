using FluentAssertions;

using GymTracker.Application.Catalog;

namespace GymTracker.Application.UnitTests.Catalog;

public sealed class CatalogFallbackPolicyTests
{
    [Fact]
    public async Task GetAsync_ShouldReturnDegradedState_WhenMarkedAsDegraded()
    {
        var store = new InMemoryAvailabilityStore();
        var service = new CatalogAvailabilityService(store);

        await store.MarkDegradedAsync(DateTimeOffset.Parse("2026-05-05T10:00:00Z"));
        var status = await service.GetAsync();

        status.IsStale.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnHealthyState_WhenMarkedHealthy()
    {
        var store = new InMemoryAvailabilityStore();
        var service = new CatalogAvailabilityService(store);

        await store.MarkHealthyAsync(DateTimeOffset.Parse("2026-05-05T10:00:00Z"));
        var status = await service.GetAsync();

        status.IsStale.Should().BeFalse();
    }

    private sealed class InMemoryAvailabilityStore : ICatalogAvailabilityStore
    {
        private CatalogAvailabilityDto _state = new(true, DateTimeOffset.MinValue);

        public Task<CatalogAvailabilityDto> GetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_state);
        }

        public Task MarkHealthyAsync(DateTimeOffset updatedAt, CancellationToken cancellationToken = default)
        {
            _state = new CatalogAvailabilityDto(false, updatedAt);
            return Task.CompletedTask;
        }

        public Task MarkDegradedAsync(DateTimeOffset updatedAt, CancellationToken cancellationToken = default)
        {
            _state = new CatalogAvailabilityDto(true, updatedAt);
            return Task.CompletedTask;
        }
    }
}
