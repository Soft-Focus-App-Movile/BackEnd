using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Interfaces.REST.Resources;

namespace SoftFocusBackend.Crisis.Interfaces.REST.Transform;

public static class UpdateAlertSeverityCommandFromResourceAssembler
{
    public static UpdateAlertSeverityCommand ToCommandFromResource(
        UpdateAlertSeverityResource resource,
        string alertId)
    {
        var severity = Enum.Parse<AlertSeverity>(resource.Severity, ignoreCase: true);

        return new UpdateAlertSeverityCommand(
            AlertId: alertId,
            Severity: severity
        );
    }
}
