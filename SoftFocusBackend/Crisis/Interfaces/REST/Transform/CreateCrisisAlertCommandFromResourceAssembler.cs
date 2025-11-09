using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Interfaces.REST.Resources;

namespace SoftFocusBackend.Crisis.Interfaces.REST.Transform;

public static class CreateCrisisAlertCommandFromResourceAssembler
{
    public static CreateCrisisAlertCommand ToCommandFromResource(
        CreateCrisisAlertResource resource,
        string patientId)
    {
        return new CreateCrisisAlertCommand(
            PatientId: patientId,
            Severity: AlertSeverity.Critical,
            TriggerSource: "MANUAL_BUTTON",
            TriggerReason: "User pressed crisis button",
            Latitude: resource.Latitude,
            Longitude: resource.Longitude
        );
    }
}
