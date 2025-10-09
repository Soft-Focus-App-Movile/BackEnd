using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Repositories;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public class FavoriteQueryService : IFavoriteQueryService
{
    private readonly IUserFavoriteRepository _favoriteRepository;
    private readonly ILogger<FavoriteQueryService> _logger;

    public FavoriteQueryService(
        IUserFavoriteRepository favoriteRepository,
        ILogger<FavoriteQueryService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<List<UserFavorite>> GetFavoritesAsync(GetFavoritesQuery query)
    {
        query.Validate();

        if (query.ContentTypeFilter.HasValue && query.EmotionFilter.HasValue)
        {
            var favorites = await _favoriteRepository.FindByUserIdAndTypeAsync(
                query.UserId, query.ContentTypeFilter.Value);
            return favorites.Where(f => f.Content.EmotionalTags.Contains(query.EmotionFilter.Value)).ToList();
        }
        else if (query.ContentTypeFilter.HasValue)
        {
            var favorites = await _favoriteRepository.FindByUserIdAndTypeAsync(
                query.UserId, query.ContentTypeFilter.Value);
            return favorites.ToList();
        }
        else if (query.EmotionFilter.HasValue)
        {
            var favorites = await _favoriteRepository.FindByUserIdAndEmotionAsync(
                query.UserId, query.EmotionFilter.Value);
            return favorites.ToList();
        }
        else
        {
            var favorites = await _favoriteRepository.FindByUserIdAsync(query.UserId);
            return favorites.ToList();
        }
    }
}
