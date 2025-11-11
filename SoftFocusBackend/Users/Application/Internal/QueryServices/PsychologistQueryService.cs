using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Domain.Repositories;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Users.Application.Internal.QueryServices;

public class PsychologistQueryService : IPsychologistQueryService
{
    private readonly IPsychologistRepository _psychologistRepository;
    private readonly ITherapeuticRelationshipRepository _therapyRepository;
    private readonly ICrisisAlertRepository _crisisRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly ILogger<PsychologistQueryService> _logger;

    public PsychologistQueryService(
        IPsychologistRepository psychologistRepository,
        ITherapeuticRelationshipRepository therapyRepository,
        ICrisisAlertRepository crisisRepository,
        ICheckInRepository checkInRepository,
        ILogger<PsychologistQueryService> logger)
    {
        _psychologistRepository = psychologistRepository ?? throw new ArgumentNullException(nameof(psychologistRepository));
        _therapyRepository = therapyRepository ?? throw new ArgumentNullException(nameof(therapyRepository));
        _crisisRepository = crisisRepository ?? throw new ArgumentNullException(nameof(crisisRepository));
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
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

            // 1. Obtener pacientes activos
            var allRelationships = await _therapyRepository.GetByPsychologistIdAsync(query.UserId);
            var activePatients = allRelationships
                .Where(r => r.Status == TherapyStatus.Active && r.IsActive)
                .ToList();
            var activePatientsCount = activePatients.Count;

            // 2. Obtener alertas de crisis pendientes
            var pendingCrisisAlerts = await _crisisRepository.CountPendingAlertsByPsychologistAsync(query.UserId);

            // 3. Calcular check-ins completados hoy
            var todayCheckInsCompleted = 0;
            var totalEmotionalLevel = 0.0;
            var totalAdherenceDays = 0;
            var completedCheckIns = 0;

            foreach (var patient in activePatients)
            {
                // Check-in de hoy
                var todayCheckIn = await _checkInRepository.FindTodayCheckInByUserIdAsync(patient.PatientId);
                if (todayCheckIn != null)
                {
                    todayCheckInsCompleted++;
                    totalEmotionalLevel += todayCheckIn.EmotionalLevel;
                }

                // Adherencia (últimos 30 días)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var patientCheckIns = await _checkInRepository.FindByUserIdAsync(
                    patient.PatientId,
                    thirtyDaysAgo,
                    DateTime.UtcNow,
                    1,
                    1000);

                completedCheckIns += patientCheckIns.Count;
                totalAdherenceDays += 30; // 30 días por paciente
            }

            // 4. Calcular adherencia promedio
            var averageAdherenceRate = totalAdherenceDays > 0
                ? Math.Round((completedCheckIns / (double)totalAdherenceDays) * 100, 2)
                : 0;

            // 5. Pacientes nuevos este mes
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var newPatientsThisMonth = allRelationships
                .Count(r => r.StartDate >= startOfMonth && r.Status == TherapyStatus.Active);

            // 6. Promedio de estado emocional
            var averageEmotionalLevel = todayCheckInsCompleted > 0
                ? Math.Round(totalEmotionalLevel / todayCheckInsCompleted, 2)
                : 0;

            var stats = new PsychologistStats(
                activePatientsCount: activePatientsCount,
                pendingCrisisAlerts: pendingCrisisAlerts,
                todayCheckInsCompleted: todayCheckInsCompleted,
                averageAdherenceRate: averageAdherenceRate,
                newPatientsThisMonth: newPatientsThisMonth,
                averageEmotionalLevel: averageEmotionalLevel);

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