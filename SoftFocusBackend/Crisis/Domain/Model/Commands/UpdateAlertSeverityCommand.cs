using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Crisis.Domain.Model.Commands;

public record UpdateAlertSeverityCommand(
    string AlertId,
    AlertSeverity Severity
);
