using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public interface IFavoriteQueryService
{
    Task<List<UserFavorite>> GetFavoritesAsync(GetFavoritesQuery query);
}
