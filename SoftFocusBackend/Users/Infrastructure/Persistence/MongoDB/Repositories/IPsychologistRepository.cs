using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

public interface IPsychologistRepository : IBaseRepository<PsychologistUser>
{
    Task<PsychologistUser?> FindByInvitationCodeAsync(string invitationCode);
    Task<PsychologistUser?> FindByLicenseNumberAsync(string licenseNumber);
    Task<bool> ExistsWithLicenseNumberAsync(string licenseNumber, string? excludeUserId = null);
    Task<(List<PsychologistUser> Psychologists, int TotalCount)> FindPsychologistsForDirectoryAsync(
        int page, int pageSize, List<PsychologySpecialty>? specialties = null, string? city = null,
        double? minRating = null, bool? isAcceptingNewPatients = null, List<string>? languages = null,
        string? searchTerm = null, string? sortBy = null, bool sortDescending = false);
    Task<List<PsychologistUser>> FindVerifiedPsychologistsAsync();
    Task<List<PsychologistUser>> FindPendingVerificationAsync();
    Task<List<PsychologistUser>> FindWithExpiredCodesAsync();
    Task RegenerateExpiredCodesAsync();
}