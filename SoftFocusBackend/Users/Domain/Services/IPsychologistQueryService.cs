using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IPsychologistQueryService
{
    Task<PsychologistStats?> HandleGetPsychologistStatsAsync(GetPsychologistStatsQuery query);
    Task<(List<PsychologistUser> Psychologists, int TotalCount)> HandleGetPsychologistsDirectoryAsync(GetPsychologistsDirectoryQuery query);
    Task<PsychologistUser?> HandleGetPsychologistByIdAsync(string psychologistId);
    Task<PsychologistUser?> HandleGetPsychologistByInvitationCodeAsync(string invitationCode);
}