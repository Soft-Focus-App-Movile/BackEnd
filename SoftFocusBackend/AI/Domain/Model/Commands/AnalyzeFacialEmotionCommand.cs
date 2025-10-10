namespace SoftFocusBackend.AI.Domain.Model.Commands;

public record AnalyzeFacialEmotionCommand
{
    public string UserId { get; init; }
    public byte[] ImageBytes { get; init; }
    public bool AutoCheckIn { get; init; }
    public DateTime RequestedAt { get; init; }

    public AnalyzeFacialEmotionCommand(string userId, byte[] imageBytes, bool autoCheckIn = true)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("ImageBytes is required", nameof(imageBytes));

        const int maxSizeBytes = 5 * 1024 * 1024; // 5 MB
        if (imageBytes.Length > maxSizeBytes)
            throw new ArgumentException($"Image size cannot exceed {maxSizeBytes / 1024 / 1024} MB", nameof(imageBytes));

        UserId = userId;
        ImageBytes = imageBytes;
        AutoCheckIn = autoCheckIn;
        RequestedAt = DateTime.UtcNow;
    }

    public string GetAuditString()
    {
        return $"User {UserId} requested facial emotion analysis (Size: {ImageBytes.Length / 1024} KB, AutoCheckIn: {AutoCheckIn}) at {RequestedAt:yyyy-MM-dd HH:mm:ss}";
    }
}
