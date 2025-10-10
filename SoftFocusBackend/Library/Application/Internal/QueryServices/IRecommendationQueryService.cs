using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public interface IRecommendationQueryService
{
    Task<(WeatherCondition Weather, List<ContentItem> Places)> GetRecommendedPlacesAsync(
        GetRecommendedPlacesQuery query);
    Task<List<ContentItem>> GetRecommendedContentAsync(GetRecommendedContentQuery query);
    Task<List<ContentItem>> GetContentByEmotionAsync(GetContentByEmotionQuery query);
}
