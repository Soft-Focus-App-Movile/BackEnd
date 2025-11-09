using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Queries;

namespace SoftFocusBackend.Crisis.Application.Internal.QueryServices;

public interface ICrisisAlertQueryService
{
    Task<CrisisAlert?> Handle(GetAlertByIdQuery query);
    Task<IEnumerable<CrisisAlert>> Handle(GetPsychologistAlertsQuery query);
    Task<IEnumerable<CrisisAlert>> Handle(GetPatientAlertsQuery query);
    Task<int> GetPendingAlertCount(string psychologistId);
}
