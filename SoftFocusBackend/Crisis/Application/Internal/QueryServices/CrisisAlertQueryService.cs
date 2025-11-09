using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Queries;
using SoftFocusBackend.Crisis.Domain.Repositories;

namespace SoftFocusBackend.Crisis.Application.Internal.QueryServices;

public class CrisisAlertQueryService : ICrisisAlertQueryService
{
    private readonly ICrisisAlertRepository _crisisAlertRepository;

    public CrisisAlertQueryService(ICrisisAlertRepository crisisAlertRepository)
    {
        _crisisAlertRepository = crisisAlertRepository;
    }

    public async Task<CrisisAlert?> Handle(GetAlertByIdQuery query)
    {
        return await _crisisAlertRepository.FindByIdAsync(query.AlertId);
    }

    public async Task<IEnumerable<CrisisAlert>> Handle(GetPsychologistAlertsQuery query)
    {
        return await _crisisAlertRepository.FindByPsychologistIdAsync(
            query.PsychologistId,
            query.Severity,
            query.Status,
            query.Limit
        );
    }

    public async Task<IEnumerable<CrisisAlert>> Handle(GetPatientAlertsQuery query)
    {
        return await _crisisAlertRepository.FindByPatientIdAsync(query.PatientId, query.Limit);
    }

    public async Task<int> GetPendingAlertCount(string psychologistId)
    {
        return await _crisisAlertRepository.CountPendingAlertsByPsychologistAsync(psychologistId);
    }
}
