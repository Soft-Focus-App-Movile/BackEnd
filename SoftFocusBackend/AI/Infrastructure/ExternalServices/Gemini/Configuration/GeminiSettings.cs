namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.Gemini.Configuration;

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
    public int MaxTokens { get; set; } = 2048;
    public double Temperature { get; set; } = 0.7;
    public string ApiUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models/";
    public string SystemPrompt { get; set; } = string.Empty;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(Model) &&
               !string.IsNullOrWhiteSpace(ApiUrl) &&
               MaxTokens > 0 &&
               Temperature >= 0 && Temperature <= 2;
    }

    public string GetGenerateContentUrl()
    {
        return $"{ApiUrl}{Model}:generateContent?key={ApiKey}";
    }
}
