using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;

namespace GymTracker.Application.Admin.Exercises;

public sealed class ExerciseRelationsValidator : IExerciseRelationsValidator
{
    private readonly IMuscleRepository _muscleRepository;
    private readonly IMeasurementTypeRepository _measurementTypeRepository;

    public ExerciseRelationsValidator(
        IMuscleRepository muscleRepository,
        IMeasurementTypeRepository measurementTypeRepository)
    {
        _muscleRepository = muscleRepository;
        _measurementTypeRepository = measurementTypeRepository;
    }

    public async Task ValidateAsync(UpsertExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var primaryIds = request.PrimaryMuscleIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (primaryIds.Length == 0)
        {
            throw new ArgumentException("At least one primary muscle is required.", nameof(request.PrimaryMuscleIds));
        }

        var secondaryIds = request.SecondaryMuscleIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var allMuscleIds = primaryIds
            .Concat(secondaryIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var muscles = await _muscleRepository.ListAsync(includeDeleted: false, cancellationToken);
        var activeMuscleRefs = muscles
            .Where(item => !item.IsDeleted)
            .SelectMany(item => new[] { item.Id, item.Code })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unknownMuscles = allMuscleIds.Where(id => !activeMuscleRefs.Contains(id)).ToArray();
        if (unknownMuscles.Length > 0)
        {
            throw new ArgumentException("Exercise references inactive or missing muscles.", nameof(request.PrimaryMuscleIds));
        }

        var measurementRefs = request.MeasurementTypeIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (measurementRefs.Length == 0)
        {
            throw new ArgumentException("Exercise must include at least one measurement type.", nameof(request.MeasurementTypeIds));
        }

        var assignable = await _measurementTypeRepository.ListAssignableAsync(cancellationToken);
        var activeMeasurementRefs = assignable
            .SelectMany(item => new[] { item.Id, item.Code })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unknownMeasurementRefs = measurementRefs
            .Where(id => !activeMeasurementRefs.Contains(id))
            .ToArray();

        if (unknownMeasurementRefs.Length > 0)
        {
            throw new ArgumentException("Exercise references an inactive or missing measurement type.", nameof(request.MeasurementTypeIds));
        }
    }
}