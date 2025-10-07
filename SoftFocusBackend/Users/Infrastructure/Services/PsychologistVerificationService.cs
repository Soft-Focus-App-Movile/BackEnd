using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Services;

namespace SoftFocusBackend.Users.Infrastructure.Services;

public class PsychologistVerificationService : IPsychologistVerificationService
{
    private readonly ILogger<PsychologistVerificationService> _logger;
    private static readonly List<string> ValidColleges = new()
    {
        "Colegio de Psicólogos del Perú",
        "Colegio de Psicólogos de Lima",
        "Colegio de Psicólogos de Arequipa",
        "Colegio de Psicólogos de La Libertad",
        "Colegio de Psicólogos de Cusco",
        "Colegio de Psicólogos de Piura",
        "Colegio de Psicólogos de Junín",
        "Colegio de Psicólogos de Lambayeque"
    };

    public PsychologistVerificationService(ILogger<PsychologistVerificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ValidateLicenseNumberAsync(string licenseNumber, string professionalCollege)
    {
        await Task.CompletedTask;
        
        try
        {
            if (string.IsNullOrWhiteSpace(licenseNumber) || string.IsNullOrWhiteSpace(professionalCollege))
                return false;

            var normalizedCollege = professionalCollege.Trim();
            if (!ValidColleges.Contains(normalizedCollege))
                return false;

            if (normalizedCollege.Contains("Lima"))
                return ValidateLimaLicenseFormat(licenseNumber);
            
            if (normalizedCollege.Contains("Arequipa"))
                return ValidateArequipaLicenseFormat(licenseNumber);

            return ValidateGenericLicenseFormat(licenseNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license number: {LicenseNumber} for college: {College}", 
                licenseNumber, professionalCollege);
            return false;
        }
    }

    public async Task<bool> IsValidProfessionalCollegeAsync(string collegeName, string? region = null)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(collegeName))
            return false;

        var normalizedCollege = collegeName.Trim();
        return ValidColleges.Any(vc => vc.Equals(normalizedCollege, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<string>> GetValidCollegesAsync(string? region = null)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(region))
            return ValidColleges.ToList();

        return ValidColleges
            .Where(college => college.ToLowerInvariant().Contains(region.ToLowerInvariant()))
            .ToList();
    }

    public async Task<bool> ValidateDocumentUrlAsync(string documentUrl)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(documentUrl))
            return false;

        try
        {
            var uri = new Uri(documentUrl);
            return uri.Scheme == "https" && 
                   (uri.Host.Contains("cloudinary.com") || uri.Host.Contains("res.cloudinary.com"));
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CanPsychologistBeVerifiedAsync(PsychologistUser psychologist)
    {
        await Task.CompletedTask;

        if (psychologist == null)
            return false;

        var hasRequiredInfo = !string.IsNullOrWhiteSpace(psychologist.LicenseNumber) &&
                             !string.IsNullOrWhiteSpace(psychologist.ProfessionalCollege) &&
                             psychologist.Specialties.Count > 0 &&
                             psychologist.YearsOfExperience >= 0;

        // All 3 mandatory documents are required for admin verification
        var hasAllMandatoryDocuments = !string.IsNullOrWhiteSpace(psychologist.LicenseDocumentUrl) &&
                                       !string.IsNullOrWhiteSpace(psychologist.DiplomaCertificateUrl) &&
                                       !string.IsNullOrWhiteSpace(psychologist.IdentityDocumentUrl);

        return hasRequiredInfo && hasAllMandatoryDocuments;
    }

    public async Task<string> GenerateVerificationReportAsync(PsychologistUser psychologist)
    {
        await Task.CompletedTask;
        
        var report = new List<string>
        {
            $"Verification Report for: {psychologist.FullName}",
            $"Email: {psychologist.Email}",
            $"License Number: {psychologist.LicenseNumber}",
            $"Professional College: {psychologist.ProfessionalCollege}",
            $"Specialties: {string.Join(", ", psychologist.Specialties)}",
            $"Years of Experience: {psychologist.YearsOfExperience}",
            "",
            "Documents Provided:",
            $"- License Document: {(!string.IsNullOrWhiteSpace(psychologist.LicenseDocumentUrl) ? "✓" : "✗")}",
            $"- Diploma Certificate: {(!string.IsNullOrWhiteSpace(psychologist.DiplomaCertificateUrl) ? "✓" : "✗")}",
            $"- Identity Document: {(!string.IsNullOrWhiteSpace(psychologist.IdentityDocumentUrl) ? "✓" : "✗")}",
            "",
            $"Can be verified: {(await CanPsychologistBeVerifiedAsync(psychologist) ? "YES" : "NO")}",
            $"Report generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
        };

        return string.Join(Environment.NewLine, report);
    }

    private static bool ValidateLimaLicenseFormat(string licenseNumber)
    {
        return licenseNumber.Length >= 4 && 
               licenseNumber.Length <= 8 && 
               licenseNumber.All(char.IsDigit);
    }

    private static bool ValidateArequipaLicenseFormat(string licenseNumber)
    {
        return licenseNumber.StartsWith("AQP") && 
               licenseNumber.Length == 7 && 
               licenseNumber[3..].All(char.IsDigit);
    }

    private static bool ValidateGenericLicenseFormat(string licenseNumber)
    {
        return licenseNumber.Length >= 3 && 
               licenseNumber.Length <= 10 && 
               licenseNumber.Any(char.IsDigit);
    }
}