using Microsoft.Extensions.Caching.Memory;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Infrastructure.OAuth.Services;

/// <summary>
/// In-memory cache service for managing temporary OAuth tokens
/// Tokens are stored temporarily during the two-step OAuth registration process
/// </summary>
public class OAuthTempTokenService : IOAuthTempTokenService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OAuthTempTokenService> _logger;
    private const string CacheKeyPrefix = "oauth_temp_token_";

    public OAuthTempTokenService(IMemoryCache cache, ILogger<OAuthTempTokenService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<OAuthTempToken> CreateTokenAsync(string email, string fullName, string provider)
    {
        try
        {
            var tempToken = new OAuthTempToken(email, fullName, provider, validMinutes: 15);

            var cacheKey = $"{CacheKeyPrefix}{tempToken.Token}";
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = tempToken.ExpiresAt,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, tempToken, cacheOptions);

            _logger.LogInformation("Created OAuth temp token for email: {Email}, provider: {Provider}, expires at: {ExpiresAt}",
                email, provider, tempToken.ExpiresAt);

            return Task.FromResult(tempToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OAuth temp token for email: {Email}", email);
            throw;
        }
    }

    public Task<OAuthTempToken?> ValidateAndRetrieveTokenAsync(string token)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}{token}";

            if (!_cache.TryGetValue<OAuthTempToken>(cacheKey, out var tempToken))
            {
                _logger.LogWarning("OAuth temp token not found: {Token}", token.Substring(0, Math.Min(10, token.Length)));
                return Task.FromResult<OAuthTempToken?>(null);
            }

            if (tempToken == null || tempToken.IsExpired)
            {
                _logger.LogWarning("OAuth temp token expired: {Token}", token.Substring(0, Math.Min(10, token.Length)));
                _cache.Remove(cacheKey);
                return Task.FromResult<OAuthTempToken?>(null);
            }

            _logger.LogInformation("OAuth temp token validated for email: {Email}", tempToken.Email);
            return Task.FromResult<OAuthTempToken?>(tempToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OAuth temp token");
            return Task.FromResult<OAuthTempToken?>(null);
        }
    }

    public Task RemoveTokenAsync(string token)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}{token}";
            _cache.Remove(cacheKey);

            _logger.LogInformation("Removed OAuth temp token: {Token}", token.Substring(0, Math.Min(10, token.Length)));

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing OAuth temp token");
            return Task.CompletedTask;
        }
    }
}
