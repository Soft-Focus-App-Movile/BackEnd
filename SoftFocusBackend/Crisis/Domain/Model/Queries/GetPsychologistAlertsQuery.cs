using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Crisis.Domain.Model.Queries;

public record GetPsychologistAlertsQuery(
    string PsychologistId,
    AlertSeverity? Severity = null,
    AlertStatus? Status = null,
    int? Limit = null
);
