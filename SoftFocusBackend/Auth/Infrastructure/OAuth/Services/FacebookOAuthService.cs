using SoftFocusBackend.Auth.Infrastructure.OAuth.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SoftFocusBackend.Auth.Infrastructure.OAuth.Services;

public class FacebookOAuthService : IOAuthService
{
    private readonly FacebookOAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FacebookOAuthService> _logger;

    public FacebookOAuthService(IOptions<FacebookOAuthSettings> settings, HttpClient httpClient, ILogger<FacebookOAuthService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        if (!_settings.IsValid)
        {
            throw new InvalidOperationException("Facebook OAuth settings are not properly configured");
        }
    }

    public async Task<(string Email, string FullName, string? ProfileImageUrl)?> GetUserInfoAsync(string accessToken)
    {
        try
        {
            _logger.LogDebug("Getting user info from Facebook with access token");

            var response = await _httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,email,name,picture&access_token={accessToken}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info from Facebook: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(content);

            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogWarning("Invalid user info received from Facebook");
                return null;
            }

            _logger.LogInformation("Successfully retrieved user info from Facebook for email: {Email}", userInfo.Email);

            var profileImageUrl = userInfo.Picture?.Data?.Url;

            return (userInfo.Email, userInfo.Name ?? string.Empty, profileImageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info from Facebook");
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://graph.facebook.com/me?access_token={accessToken}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Facebook token");
            return false;
        }
    }

    public string GetProviderName() => "facebook";

    private class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        public int Height { get; set; }
        public bool Is_silhouette { get; set; }
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; }
    }
}