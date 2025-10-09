namespace SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string ProfileImagesFolder { get; set; } = string.Empty;
    public string EmotionAnalysesFolder { get; set; } = string.Empty;
    public string PsychologistDocumentsFolder { get; set; } = string.Empty;
    public int MaxFileSizeBytes { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    public bool IsValid =>
        !string.IsNullOrEmpty(CloudName) &&
        !string.IsNullOrEmpty(ApiKey) &&
        !string.IsNullOrEmpty(ApiSecret);
}