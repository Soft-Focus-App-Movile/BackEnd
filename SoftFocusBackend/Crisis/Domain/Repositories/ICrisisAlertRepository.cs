using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Crisis.Domain.Repositories;

public interface ICrisisAlertRepository : IBaseRepository<CrisisAlert>
{
    Task<IEnumerable<CrisisAlert>> FindByPsychologistIdAsync(
        string psychologistId,
        AlertSeverity? severity = null,
        AlertStatus? status = null,
        int? limit = null);

    Task<IEnumerable<CrisisAlert>> FindByPatientIdAsync(string patientId, int? limit = null);

    Task<IEnumerable<CrisisAlert>> FindPendingCriticalAlertsAsync(string psychologistId);

    Task<int> CountPendingAlertsByPsychologistAsync(string psychologistId, AlertSeverity? severity = null);
}
