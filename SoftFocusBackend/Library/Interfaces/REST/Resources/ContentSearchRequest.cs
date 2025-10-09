using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Library.Interfaces.REST.Resources;

public class ContentSearchRequest
{
    [Required(ErrorMessage = "Query is required")]
    public string Query { get; set; } = string.Empty;

    [Required(ErrorMessage = "ContentType is required")]
    public string ContentType { get; set; } = string.Empty;

    public string? EmotionFilter { get; set; }

    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; set; } = 20;
}

public class FavoriteRequest
{
    [Required] public string ContentId { get; set; } = string.Empty;
    [Required] public string ContentType { get; set; } = string.Empty;
}

public class AssignmentRequest
{
    [Required] public List<string> PatientIds { get; set; } = new();
    [Required] public string ContentId { get; set; } = string.Empty;
    [Required] public string ContentType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
