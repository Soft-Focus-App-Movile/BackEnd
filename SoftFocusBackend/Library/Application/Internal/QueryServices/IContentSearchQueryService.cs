using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public interface IContentSearchQueryService
{
    Task<List<ContentItem>> SearchContentAsync(SearchContentQuery query);
}
