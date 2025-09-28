using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IInvitationCodeService
{
    Task<InvitationCode> GenerateUniqueCodeAsync();
    Task<bool> IsCodeUniqueAsync(string code);
    Task<bool> ShouldRegenerateCodeAsync(InvitationCode currentCode);
    Task<string?> FindPsychologistByCodeAsync(string invitationCode);
    Task RegenerateExpiredCodesAsync();
    Task<int> GetActiveCodesCountAsync();
}