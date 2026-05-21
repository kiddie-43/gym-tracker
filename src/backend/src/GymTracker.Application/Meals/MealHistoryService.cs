namespace GymTracker.Application.Meals;

public sealed class MealHistoryService
{
    private readonly IMealLogRepository _mealLogRepository;

    public MealHistoryService(IMealLogRepository mealLogRepository)
    {
        _mealLogRepository = mealLogRepository;
    }

    public async Task<MealHistoryPageResponse> GetHistoryAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var all = await _mealLogRepository.ListByUserAsync(userId, cancellationToken);
        var ordered = all.OrderByDescending(item => item.LoggedDate).ThenBy(item => item.SlotType, StringComparer.OrdinalIgnoreCase);

        var total = ordered.Count();
        var items = ordered
            .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Select(item => new MealHistoryItemResponse(item.Id, item.LoggedDate, item.SlotType, item.Items.Count))
            .ToArray();

        return new MealHistoryPageResponse(items, total, Math.Max(page, 1), Math.Max(pageSize, 1));
    }
}

public sealed record MealHistoryItemResponse(string Id, DateOnly LoggedDate, string SlotType, int ItemCount);

public sealed record MealHistoryPageResponse(IReadOnlyCollection<MealHistoryItemResponse> Items, int TotalCount, int Page, int PageSize);
