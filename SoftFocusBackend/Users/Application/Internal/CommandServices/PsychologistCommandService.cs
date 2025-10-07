using SoftFocusBackend.Users.Application.ACL.Services;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Users.Application.Internal.CommandServices;

public class PsychologistCommandService : IPsychologistCommandService
{
    private readonly IPsychologistRepository _psychologistRepository;
    private readonly IPsychologistVerificationService _verificationService;
    private readonly IInvitationCodeService _invitationCodeService;
    private readonly IAuthNotificationService _authNotificationService;
    private readonly ILogger<PsychologistCommandService> _logger;

    public PsychologistCommandService(
        IPsychologistRepository psychologistRepository,
        IPsychologistVerificationService verificationService,
        IInvitationCodeService invitationCodeService,
        IAuthNotificationService authNotificationService,
        ILogger<PsychologistCommandService> logger)
    {
        _psychologistRepository = psychologistRepository ?? throw new ArgumentNullException(nameof(psychologistRepository));
        _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        _invitationCodeService = invitationCodeService ?? throw new ArgumentNullException(nameof(invitationCodeService));
        _authNotificationService = authNotificationService ?? throw new ArgumentNullException(nameof(authNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PsychologistUser?> HandleUpdateVerificationAsync(UpdatePsychologistVerificationCommand command)
    {
        try
        {
            _logger.LogInformation("Processing update psychologist verification command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid update verification command for psychologist: {UserId}", command.UserId);
                return null;
            }

            var psychologist = await _psychologistRepository.FindByIdAsync(command.UserId);
            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {UserId}", command.UserId);
                return null;
            }

            if (!await _verificationService.IsValidProfessionalCollegeAsync(command.ProfessionalCollege, command.CollegeRegion))
            {
                _logger.LogWarning("Invalid professional college: {College} - {Region}", command.ProfessionalCollege, command.CollegeRegion);
                return null;
            }

            if (!await _verificationService.ValidateLicenseNumberAsync(command.LicenseNumber, command.ProfessionalCollege))
            {
                _logger.LogWarning("Invalid license number: {LicenseNumber} for college: {College}", command.LicenseNumber, command.ProfessionalCollege);
                return null;
            }

            psychologist.UpdateProfessionalInfo(
                command.LicenseNumber,
                command.ProfessionalCollege,
                command.CollegeRegion,
                command.Specialties,
                command.YearsOfExperience,
                command.University,
                command.GraduationYear,
                command.Degree);

            // Validate document URLs before updating
            if (!string.IsNullOrWhiteSpace(command.LicenseDocumentUrl))
            {
                if (!await _verificationService.ValidateDocumentUrlAsync(command.LicenseDocumentUrl))
                {
                    _logger.LogWarning("Invalid license document URL for psychologist: {UserId}", command.UserId);
                    return null;
                }
            }

            if (!string.IsNullOrWhiteSpace(command.DiplomaCertificateUrl))
            {
                if (!await _verificationService.ValidateDocumentUrlAsync(command.DiplomaCertificateUrl))
                {
                    _logger.LogWarning("Invalid diploma certificate URL for psychologist: {UserId}", command.UserId);
                    return null;
                }
            }

            if (!string.IsNullOrWhiteSpace(command.IdentityDocumentUrl))
            {
                if (!await _verificationService.ValidateDocumentUrlAsync(command.IdentityDocumentUrl))
                {
                    _logger.LogWarning("Invalid identity document URL for psychologist: {UserId}", command.UserId);
                    return null;
                }
            }

            if (command.AdditionalCertificatesUrls != null && command.AdditionalCertificatesUrls.Count > 0)
            {
                foreach (var url in command.AdditionalCertificatesUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url) && !await _verificationService.ValidateDocumentUrlAsync(url))
                    {
                        _logger.LogWarning("Invalid additional certificate URL for psychologist: {UserId}", command.UserId);
                        return null;
                    }
                }
            }

            psychologist.UpdateVerificationDocuments(
                command.LicenseDocumentUrl,
                command.DiplomaCertificateUrl,
                command.IdentityDocumentUrl,
                command.AdditionalCertificatesUrls);

            _psychologistRepository.Update(psychologist);

            _logger.LogInformation("Psychologist verification updated successfully: {UserId}", psychologist.Id);
            return psychologist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating psychologist verification: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<string?> HandleRegenerateInvitationCodeAsync(RegenerateInvitationCodeCommand command)
    {
        try
        {
            _logger.LogInformation("Processing regenerate invitation code command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid regenerate invitation code command for psychologist: {UserId}", command.UserId);
                return null;
            }

            var psychologist = await _psychologistRepository.FindByIdAsync(command.UserId);
            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {UserId}", command.UserId);
                return null;
            }

            if (!psychologist.IsVerified)
            {
                _logger.LogWarning("Cannot regenerate code for unverified psychologist: {UserId}", command.UserId);
                return null;
            }

            psychologist.GenerateNewInvitationCode();
            _psychologistRepository.Update(psychologist);

            _logger.LogInformation("Invitation code regenerated successfully for psychologist: {UserId} - New code: {Code}", 
                psychologist.Id, psychologist.InvitationCode);

            return psychologist.InvitationCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating invitation code: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<PsychologistUser?> HandleUpdateProfessionalProfileAsync(UpdateProfessionalProfileCommand command)
    {
        try
        {
            _logger.LogInformation("Processing update professional profile command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid update professional profile command for psychologist: {UserId}", command.UserId);
                return null;
            }

            var psychologist = await _psychologistRepository.FindByIdAsync(command.UserId);
            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {UserId}", command.UserId);
                return null;
            }

            psychologist.UpdateProfessionalProfile(
                command.ProfessionalBio,
                command.IsAcceptingNewPatients,
                command.MaxPatientsCapacity,
                command.TargetAudience,
                command.Languages,
                command.BusinessName,
                command.BusinessAddress);

            if (command.IsProfileVisibleInDirectory.HasValue)
            {
                psychologist.UpdateDirectoryVisibility(command.IsProfileVisibleInDirectory.Value);
            }

            if (command.AllowsDirectMessages.HasValue)
            {
                psychologist.AllowsDirectMessages = command.AllowsDirectMessages.Value;
                psychologist.UpdatedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(command.BankAccount))
            {
                psychologist.BankAccount = command.BankAccount;
                psychologist.UpdatedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(command.PaymentMethods))
            {
                psychologist.PaymentMethods = command.PaymentMethods;
                psychologist.UpdatedAt = DateTime.UtcNow;
            }

            _psychologistRepository.Update(psychologist);

            _logger.LogInformation("Professional profile updated successfully: {UserId}", psychologist.Id);
            return psychologist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating professional profile: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<bool> HandleVerifyPsychologistAsync(VerifyPsychologistCommand command)
    {
        try
        {
            _logger.LogInformation("Processing verify psychologist command: {AuditInfo}", command.GetAuditString());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid verify psychologist command for psychologist: {UserId}", command.UserId);
                return false;
            }

            var psychologist = await _psychologistRepository.FindByIdAsync(command.UserId);
            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {UserId}", command.UserId);
                return false;
            }

            if (command.IsApproved)
            {
                if (!await _verificationService.CanPsychologistBeVerifiedAsync(psychologist))
                {
                    _logger.LogWarning("Psychologist cannot be verified due to incomplete information: {UserId}", command.UserId);
                    return false;
                }

                psychologist.Verify(command.VerifiedBy, command.VerificationNotes);

                if (string.IsNullOrWhiteSpace(psychologist.InvitationCode))
                {
                    psychologist.GenerateNewInvitationCode();
                }

                _logger.LogInformation("Psychologist verified successfully: {UserId} by {VerifiedBy}", 
                    psychologist.Id, command.VerifiedBy);
            }
            else
            {
                psychologist.IsVerified = false;
                psychologist.VerificationNotes = command.VerificationNotes;
                psychologist.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Psychologist verification rejected: {UserId} by {VerifiedBy} - Reason: {Notes}", 
                    psychologist.Id, command.VerifiedBy, command.VerificationNotes);
            }

            _psychologistRepository.Update(psychologist);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying psychologist: {UserId}", command.UserId);
            return false;
        }
    }
}