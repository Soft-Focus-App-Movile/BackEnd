using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Users.Application.Internal.QueryServices;

public class PsychologistQueryService : IPsychologistQueryService
{
    private readonly IPsychologistRepository _psychologistRepository;
    private readonly ILogger<PsychologistQueryService> _logger;

    public PsychologistQueryService(
        IPsychologistRepository psychologistRepository,
        ILogger<PsychologistQueryService> logger)
    {
        _psychologistRepository = psychologistRepository ?? throw new ArgumentNullException(nameof(psychologistRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PsychologistStats?> HandleGetPsychologistStatsAsync(GetPsychologistStatsQuery query)
    {
        try
        {
            _logger.LogDebug("Processing get psychologist stats query: {AuditInfo}", query.GetAuditString());

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get psychologist stats query for user: {UserId}", query.UserId);
                return null;
            }

            var psychologist = await _psychologistRepository.FindByIdAsync(query.UserId);
            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {UserId}", query.UserId);
                return null;
            }

            var stats = new PsychologistStats(
                connectedPatientsCount: psychologist.CurrentPatientsCount ?? 0,
                totalCheckInsReceived: 0, 
                crisisAlertsHandled: 0,   
                averageResponseTime: TimeSpan.FromMinutes(15), 
                isAcceptingNewPatients: psychologist.IsAcceptingNewPatients,
                lastActivityDate: psychologist.LastLogin,
                joinedDate: psychologist.CreatedAt,
                averageRating: psychologist.AverageRating,
                totalReviews: psychologist.TotalReviews ?? 0);

            _logger.LogDebug("Psychologist stats retrieved successfully: {UserId}", query.UserId);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving psychologist stats: {UserId}", query.UserId);
            return null;
        }
    }

    public async Task<(List<PsychologistUser> Psychologists, int TotalCount)> HandleGetPsychologistsDirectoryAsync(GetPsychologistsDirectoryQuery query)
    {
        try
        {
            _logger.LogDebug("Processing get psychologists directory query: {AuditInfo}", query.GetAuditString());

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get psychologists directory query");
                return (new List<PsychologistUser>(), 0);
            }

            var (psychologists, totalCount) = await _psychologistRepository.FindPsychologistsForDirectoryAsync(
                query.Page,
                query.PageSize,
                query.Specialties,
                query.City,
                query.MinRating,
                query.IsAcceptingNewPatients,
                query.Languages,
                query.SearchTerm,
                query.SortBy,
                query.SortDescending);

            _logger.LogDebug("Psychologists directory retrieved successfully: {Count} psychologists, {TotalCount} total", 
                psychologists.Count, totalCount);
            return (psychologists, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving psychologists directory");
            return (new List<PsychologistUser>(), 0);
        }
    }

    public async Task<PsychologistUser?> HandleGetPsychologistByIdAsync(string psychologistId)
    {
        try
        {
            _logger.LogDebug("Processing get psychologist by id: {PsychologistId}", psychologistId);

            if (string.IsNullOrWhiteSpace(psychologistId))
            {
                _logger.LogWarning("Invalid psychologist id");
                return null;
            }

            var psychologist = await _psychologistRepository.FindByIdAsync(psychologistId);

            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {PsychologistId}", psychologistId);
                return null;
            }

            if (!psychologist.IsProfileVisibleInDirectory)
            {
                _logger.LogWarning("Psychologist profile not visible in directory: {PsychologistId}", psychologistId);
                return null;
            }

            _logger.LogDebug("Psychologist retrieved successfully: {PsychologistId}", psychologistId);
            return psychologist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving psychologist by id: {PsychologistId}", psychologistId);
            return null;
        }
    }

    public async Task<PsychologistUser?> HandleGetPsychologistByInvitationCodeAsync(string invitationCode)
    {
        try
        {
            _logger.LogDebug("Processing get psychologist by invitation code: {Code}", invitationCode);

            if (string.IsNullOrWhiteSpace(invitationCode))
            {
                _logger.LogWarning("Invalid invitation code");
                return null;
            }

            var psychologist = await _psychologistRepository.FindByInvitationCodeAsync(invitationCode);

            if (psychologist == null)
            {
                _logger.LogDebug("Psychologist not found for invitation code: {Code}", invitationCode);
                return null;
            }

            if (psychologist.IsInvitationCodeExpired())
            {
                _logger.LogWarning("Invitation code expired for psychologist: {PsychologistId}", psychologist.Id);
                return null;
            }

            _logger.LogDebug("Psychologist found by invitation code: {PsychologistId} - {Code}", 
                psychologist.Id, invitationCode);
            return psychologist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving psychologist by invitation code: {Code}", invitationCode);
            return null;
        }
    }
}