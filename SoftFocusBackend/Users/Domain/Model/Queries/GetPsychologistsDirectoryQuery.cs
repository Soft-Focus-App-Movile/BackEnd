using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Queries;

public record GetPsychologistsDirectoryQuery
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public List<PsychologySpecialty>? Specialties { get; init; }
    public string? City { get; init; }
    public double? MinRating { get; init; }
    public bool? IsAcceptingNewPatients { get; init; }
    public List<string>? Languages { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public DateTime RequestedAt { get; init; }

    public GetPsychologistsDirectoryQuery(int page = 1, int pageSize = 20,
        List<PsychologySpecialty>? specialties = null, string? city = null,
        double? minRating = null, bool? isAcceptingNewPatients = null,
        List<string>? languages = null, string? searchTerm = null,
        string? sortBy = null, bool sortDescending = false)
    {
        Page = Math.Max(1, page);
        PageSize = Math.Clamp(pageSize, 1, 50);
        Specialties = specialties;
        City = city?.Trim();
        MinRating = minRating.HasValue ? Math.Clamp(minRating.Value, 0, 5) : null;
        IsAcceptingNewPatients = isAcceptingNewPatients;
        Languages = languages;
        SearchTerm = searchTerm?.Trim();
        SortBy = sortBy?.Trim();
        SortDescending = sortDescending;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return Page > 0 && 
               PageSize > 0 && 
               PageSize <= 50 &&
               (MinRating == null || (MinRating >= 0 && MinRating <= 5));
    }

    public int GetSkip() => (Page - 1) * PageSize;

    public string GetAuditString()
    {
        var parts = new List<string> 
        { 
            $"Page: {Page}", 
            $"PageSize: {PageSize}"
        };

        if (Specialties?.Count > 0)
            parts.Add($"Specialties: [{string.Join(", ", Specialties)}]");

        if (!string.IsNullOrWhiteSpace(City))
            parts.Add($"City: {City}");

        if (MinRating.HasValue)
            parts.Add($"MinRating: {MinRating}");

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            parts.Add($"SearchTerm: {SearchTerm}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}