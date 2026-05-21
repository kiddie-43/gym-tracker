using GymTracker.Domain.Entities;

namespace GymTracker.Application.Diets;

public sealed class DietService
{
    private readonly IDietRepository _dietRepository;

    public DietService(IDietRepository dietRepository)
    {
        _dietRepository = dietRepository;
    }

    public async Task<DietResponse> CreateAsync(string userId, CreateDietRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var diet = ToEntity(userId, request);
        await _dietRepository.AddAsync(diet, cancellationToken);

        return ToResponse(diet);
    }

    public async Task<IReadOnlyCollection<DietSummaryResponse>> ListAsync(string userId, CancellationToken cancellationToken = default)
    {
        var diets = await _dietRepository.ListByUserAsync(userId, cancellationToken);
        return diets
            .OrderBy(diet => diet.Name, StringComparer.OrdinalIgnoreCase)
            .Select(diet => new DietSummaryResponse(diet.Id, diet.Name, diet.Days.Count))
            .ToArray();
    }

    public async Task<DietResponse?> GetByIdAsync(string userId, string dietId, CancellationToken cancellationToken = default)
    {
        var diet = await _dietRepository.GetByUserAndIdAsync(userId, dietId, cancellationToken);
        return diet is null ? null : ToResponse(diet);
    }

    public async Task<DietResponse?> UpdateAsync(string userId, string dietId, CreateDietRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var existing = await _dietRepository.GetByUserAndIdAsync(userId, dietId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var updated = new Diet
        {
            Id = existing.Id,
            CreatedAt = existing.CreatedAt,
            UserId = existing.UserId,
            Name = request.Name,
            Days = request.Days.Select(ToDietDay).ToArray(),
        };

        await _dietRepository.UpdateAsync(updated, cancellationToken);
        return ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(string userId, string dietId, CancellationToken cancellationToken = default)
    {
        var existing = await _dietRepository.GetByUserAndIdAsync(userId, dietId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _dietRepository.DeleteAsync(userId, dietId, cancellationToken);
        return true;
    }

    private static void Validate(CreateDietRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("name is required.");
        }

        if (request.Days.Count == 0)
        {
            throw new ArgumentException("At least one diet day is required.");
        }
    }

    private static Diet ToEntity(string userId, CreateDietRequest request)
    {
        return new Diet
        {
            UserId = userId,
            Name = request.Name,
            Days = request.Days.Select(ToDietDay).ToArray(),
        };
    }

    private static DietDay ToDietDay(DietDayInput day)
    {
        return new DietDay
        {
            DayKey = day.DayKey,
            MealSlots = day.MealSlots.Select(slot => new MealSlot
            {
                SlotType = slot.SlotType,
                Items = slot.Items.Select(item => new MealItem
                {
                    ExternalFoodId = item.ExternalFoodId,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    Calories = item.Calories,
                }).ToArray(),
            }).ToArray(),
        };
    }

    private static DietResponse ToResponse(Diet diet)
    {
        return new DietResponse(
            diet.Id,
            diet.Name,
            diet.Days.Select(day =>
                new DietDayInput(
                    day.DayKey,
                    day.MealSlots.Select(slot =>
                        new MealSlotInput(
                            slot.SlotType,
                            slot.Items.Select(item => new MealItemInput(item.ExternalFoodId, item.Quantity, item.Unit, item.Calories)).ToArray()))
                    .ToArray()))
            .ToArray());
    }
}
