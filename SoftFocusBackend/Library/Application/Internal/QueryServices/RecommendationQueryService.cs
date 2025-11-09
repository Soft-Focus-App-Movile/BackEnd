using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Services;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public class RecommendationQueryService : IRecommendationQueryService
{
    private readonly IWeatherPlaceRecommender _placeRecommender;
    private readonly IEmotionContentMatcher _emotionMatcher;
    private readonly ITrackingIntegrationService _trackingIntegration;
    private readonly ILogger<RecommendationQueryService> _logger;

    public RecommendationQueryService(
        IWeatherPlaceRecommender placeRecommender,
        IEmotionContentMatcher emotionMatcher,
        ITrackingIntegrationService trackingIntegration,
        ILogger<RecommendationQueryService> logger)
    {
        _placeRecommender = placeRecommender;
        _emotionMatcher = emotionMatcher;
        _trackingIntegration = trackingIntegration;
        _logger = logger;
    }

    public async Task<WeatherCondition> GetRecommendedPlacesAsync(
        GetRecommendedPlacesQuery query)
    {
        query.Validate();

        var weather = await _placeRecommender.GetCurrentWeatherAsync(query.Latitude, query.Longitude);

        return weather;
    }

    public async Task<List<ContentItem>> GetRecommendedContentAsync(GetRecommendedContentQuery query)
    {
        query.Validate();

        // Intentar obtener emoción desde Tracking Context
        EmotionalTag? emotion = null;
        var trackingAvailable = await _trackingIntegration.IsTrackingContextAvailableAsync();

        if (trackingAvailable)
        {
            emotion = await _trackingIntegration.GetCurrentEmotionAsync(query.UserId);
        }

        // Si no hay emoción desde tracking, usar clima como proxy (si se proporcionó ubicación)
        if (!emotion.HasValue && query.Latitude.HasValue && query.Longitude.HasValue)
        {
            var weather = await _placeRecommender.GetCurrentWeatherAsync(
                query.Latitude.Value, query.Longitude.Value);

            emotion = weather.IsOutdoorFriendly() ? EmotionalTag.Energetic : EmotionalTag.Calm;
        }

        // Si aún no hay emoción, usar Calm por defecto
        emotion ??= EmotionalTag.Calm;

        return await _emotionMatcher.GetContentForEmotionAsync(
            emotion.Value,
            query.ContentType,
            query.Limit
        );
    }

    public async Task<List<ContentItem>> GetContentByEmotionAsync(GetContentByEmotionQuery query)
    {
        query.Validate();

        return await _emotionMatcher.GetContentForEmotionAsync(
            query.Emotion,
            query.ContentType,
            query.Limit
        );
    }
}
