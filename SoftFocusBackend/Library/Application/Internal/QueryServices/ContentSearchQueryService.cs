using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Services;

namespace SoftFocusBackend.Library.Application.Internal.QueryServices;

public class ContentSearchQueryService : IContentSearchQueryService
{
    private readonly IContentSearchService _searchService;
    private readonly ILogger<ContentSearchQueryService> _logger;

    public ContentSearchQueryService(
        IContentSearchService searchService,
        ILogger<ContentSearchQueryService> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<List<ContentItem>> SearchContentAsync(SearchContentQuery query)
    {
        query.Validate();

        return await _searchService.SearchContentAsync(
            query.Query,
            query.ContentType,
            query.EmotionFilter,
            query.Limit
        );
    }
}
