namespace SoftFocusBackend.Crisis.Domain.Model.Queries;

public record GetPatientAlertsQuery(string PatientId, int? Limit = null);
