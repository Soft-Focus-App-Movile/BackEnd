using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record PsychologistDirectoryResource
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
    public string? ProfessionalBio { get; init; }
    public List<PsychologySpecialty> Specialties { get; init; } = new();
    public int YearsOfExperience { get; init; }
    public string? City { get; init; }
    public List<string>? Languages { get; init; }
    public bool IsAcceptingNewPatients { get; init; }
    public double? AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public bool AllowsDirectMessages { get; init; }
    public List<string>? TargetAudience { get; init; }
}