namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.HuggingFace.Configuration;

public class HuggingFaceSettings
{
    public string ApiToken { get; set; } = string.Empty;
    public string ModelId { get; set; } = "j-hartmann/emotion-english-distilroberta-base";
    public string ApiUrl { get; set; } = "https://api-inference.huggingface.co/models/";

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApiToken) &&
               !string.IsNullOrWhiteSpace(ModelId) &&
               !string.IsNullOrWhiteSpace(ApiUrl);
    }

    public string GetModelUrl()
    {
        return $"{ApiUrl}{ModelId}";
    }
}
