using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IPsychologistCommandService
{
    Task<PsychologistUser?> HandleUpdateVerificationAsync(UpdatePsychologistVerificationCommand command);
    Task<string?> HandleRegenerateInvitationCodeAsync(RegenerateInvitationCodeCommand command);
    Task<PsychologistUser?> HandleUpdateProfessionalProfileAsync(UpdateProfessionalProfileCommand command);
    Task<bool> HandleVerifyPsychologistAsync(VerifyPsychologistCommand command);
}