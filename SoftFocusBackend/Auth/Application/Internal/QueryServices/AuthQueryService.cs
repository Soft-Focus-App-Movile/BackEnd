using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Domain.Model.Queries;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Auth.Domain.Services;

namespace SoftFocusBackend.Auth.Application.Internal.QueryServices;

public class AuthQueryService : IAuthQueryService
{
    private readonly IUserContextService _userContextService;
    private readonly ILogger<AuthQueryService> _logger;

    public AuthQueryService(
        IUserContextService userContextService,
        ILogger<AuthQueryService> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthenticatedUser?> HandleGetCurrentUserAsync(GetCurrentUserQuery query)
    {
        try
        {
            _logger.LogDebug("Processing get current user query: {AuditInfo}", query.GetAuditString());

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get current user query for user ID: {UserId}", query.UserId);
                return null;
            }

            var authenticatedUser = await _userContextService.GetUserByIdAsync(query.UserId);

            if (authenticatedUser == null)
            {
                _logger.LogWarning("User not found or inactive for ID: {UserId}", query.UserId);
                return null;
            }

            _logger.LogDebug("Current user retrieved successfully: {UserId} - {Email}", 
                authenticatedUser.Id, authenticatedUser.Email);

            return authenticatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get current user query for user ID: {UserId}", query.UserId);
            return null;
        }
    }
}