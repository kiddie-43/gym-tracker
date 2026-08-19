using GymTracker.Domain.ValueObjects;

namespace GymTracker.Domain.Services;

public static class ExerciseMediaRules
{
    private const int MaxItems = 8;
    private const int MaxImages = 6;
    private const int MaxVideos = 2;

    public static void ValidateCollection(IReadOnlyCollection<ExerciseMedia> mediaItems)
    {
        ArgumentNullException.ThrowIfNull(mediaItems);

        var activeItems = mediaItems
            .Where(item => item.Active && !item.IsDeleted)
            .ToArray();

        if (activeItems.Length > MaxItems)
        {
            throw new ArgumentException("An exercise cannot have more than 8 active media items.", nameof(mediaItems));
        }

        var imageCount = activeItems.Count(item => item.MediaType == ExerciseMediaType.Image);
        if (imageCount > MaxImages)
        {
            throw new ArgumentException("An exercise cannot have more than 6 active images.", nameof(mediaItems));
        }

        var videoCount = activeItems.Count(item => item.MediaType == ExerciseMediaType.Video);
        if (videoCount > MaxVideos)
        {
            throw new ArgumentException("An exercise cannot have more than 2 active videos.", nameof(mediaItems));
        }

        var primaryCount = activeItems.Count(item => item.IsPrimary);
        if (primaryCount > 1)
        {
            throw new ArgumentException("Only one active media item can be primary.", nameof(mediaItems));
        }

        var duplicatedSortOrder = activeItems
            .GroupBy(item => item.SortOrder)
            .Any(group => group.Count() > 1);

        if (duplicatedSortOrder)
        {
            throw new ArgumentException("Active media items must not share sortOrder.", nameof(mediaItems));
        }
    }
}
