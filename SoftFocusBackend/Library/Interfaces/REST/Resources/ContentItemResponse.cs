namespace SoftFocusBackend.Library.Interfaces.REST.Resources;

public class ContentItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string BackdropUrl { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string TrailerUrl { get; set; } = string.Empty;
    public List<string> EmotionalTags { get; set; } = new();
    public string ExternalUrl { get; set; } = string.Empty;

    // Music specific
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? PreviewUrl { get; set; }
    public string? SpotifyUrl { get; set; }

    // Video specific
    public string? ChannelName { get; set; }
    public string? YouTubeUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Place specific
    public string? Category { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Distance { get; set; }
    public string? PhotoUrl { get; set; }
}
