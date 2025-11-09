namespace SoftFocusBackend.Crisis.Interfaces.REST.Resources;

public record UpdateAlertStatusResource(
    string Status,
    string? PsychologistNotes
);
