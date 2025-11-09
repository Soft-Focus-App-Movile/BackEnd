using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Interfaces.REST.Resources;

namespace SoftFocusBackend.Crisis.Interfaces.REST.Transform;

public static class UpdateAlertStatusCommandFromResourceAssembler
{
    public static UpdateAlertStatusCommand ToCommandFromResource(
        UpdateAlertStatusResource resource,
        string alertId)
    {
        var status = Enum.Parse<AlertStatus>(resource.Status, ignoreCase: true);

        return new UpdateAlertStatusCommand(
            AlertId: alertId,
            Status: status,
            PsychologistNotes: resource.PsychologistNotes
        );
    }
}
