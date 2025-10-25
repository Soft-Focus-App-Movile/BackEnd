using SoftFocusBackend.Auth.Infrastructure.OAuth.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoftFocusBackend.Auth.Infrastructure.OAuth.Services;

public class GoogleOAuthService : IOAuthService
{
    private readonly GoogleOAuthSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleOAuthService> _logger;

    public GoogleOAuthService(IOptions<GoogleOAuthSettings> settings, HttpClient httpClient, ILogger<GoogleOAuthService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;

        if (!_settings.IsValid)
        {
            throw new InvalidOperationException("Google OAuth settings are not properly configured");
        }
    }

    public async Task<(string Email, string FullName, string? ProfileImageUrl)?> GetUserInfoAsync(string token)
    {
        try
        {
            _logger.LogDebug("Getting user info from Google with token");

            string? accessToken = null;

            // Check if this is a serverAuthCode (starts with "4/") or an access/id token
            if (token.StartsWith("4/"))
            {
                _logger.LogInformation("Token appears to be serverAuthCode, exchanging for access token");
                accessToken = await ExchangeAuthCodeForAccessToken(token);

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Failed to exchange serverAuthCode for access token");
                    return null;
                }
            }
            else
            {
                // Assume it's already an access token or ID token
                accessToken = token;
            }

            // Get user info using access token
            var response = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info from Google: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("DEBUG: Google API response: {Content}", content);
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content);
            _logger.LogInformation("DEBUG: Deserialized - Email: {Email}, Name: {Name}",
                userInfo?.Email ?? "null", userInfo?.Name ?? "null");

            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                _logger.LogWarning("Invalid user info received from Google");
                return null;
            }

            _logger.LogInformation("Successfully retrieved user info from Google for email: {Email}", userInfo.Email);

            return (userInfo.Email, userInfo.Name ?? string.Empty, userInfo.Picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info from Google");
            return null;
        }
    }

    private async Task<string?> ExchangeAuthCodeForAccessToken(string authCode)
    {
        try
        {
            // For Android apps, Google uses an empty redirect_uri when using server auth code
            var tokenRequest = new Dictionary<string, string>
            {
                { "code", authCode },
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "redirect_uri", "" }, // Empty for Android server auth code flow
                { "grant_type", "authorization_code" }
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to exchange auth code: {StatusCode} - {Error}", response.StatusCode, error);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Token exchange successful: {Response}", responseContent);
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);

            return tokenResponse?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging auth code for access token");
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={accessToken}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return false;
        }
    }

    public string GetProviderName() => "google";

    private class GoogleUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("verified_email")]
        public bool Verified_email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("given_name")]
        public string Given_name { get; set; } = string.Empty;

        [JsonPropertyName("family_name")]
        public string Family_name { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = string.Empty;

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = string.Empty;
    }

    private class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }
}