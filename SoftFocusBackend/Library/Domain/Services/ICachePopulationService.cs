using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Services;

public interface ICachePopulationService
{
    Task<List<ContentItem>> PopulateCacheForTypeAsync(
        ContentType contentType,
        EmotionalTag? emotion = null,
        int limit = 20);
}
