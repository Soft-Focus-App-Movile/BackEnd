using SoftFocusBackend.Users.Domain.Model.Aggregates;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IPsychologistVerificationService
{
    Task<bool> ValidateLicenseNumberAsync(string licenseNumber, string professionalCollege);
    Task<bool> IsValidProfessionalCollegeAsync(string collegeName, string? region = null);
    Task<List<string>> GetValidCollegesAsync(string? region = null);
    Task<bool> ValidateDocumentUrlAsync(string documentUrl);
    Task<bool> CanPsychologistBeVerifiedAsync(PsychologistUser psychologist);
    Task<string> GenerateVerificationReportAsync(PsychologistUser psychologist);
}