using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record PsychologistProfessionalResource
{
    [StringLength(1000, ErrorMessage = "Professional bio cannot exceed 1000 characters")]
    public string? ProfessionalBio { get; init; }

    public bool? IsAcceptingNewPatients { get; init; }

    [Range(1, 500, ErrorMessage = "Max patients capacity must be between 1 and 500")]
    public int? MaxPatientsCapacity { get; init; }

    public List<string>? TargetAudience { get; init; }

    public List<string>? Languages { get; init; }

    [StringLength(100, ErrorMessage = "Business name cannot exceed 100 characters")]
    public string? BusinessName { get; init; }

    [StringLength(200, ErrorMessage = "Business address cannot exceed 200 characters")]
    public string? BusinessAddress { get; init; }

    [StringLength(50, ErrorMessage = "Bank account cannot exceed 50 characters")]
    public string? BankAccount { get; init; }

    [StringLength(200, ErrorMessage = "Payment methods cannot exceed 200 characters")]
    public string? PaymentMethods { get; init; }

    public bool? IsProfileVisibleInDirectory { get; init; }

    public bool? AllowsDirectMessages { get; init; }
}