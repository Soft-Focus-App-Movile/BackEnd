using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Crisis.Domain.Model.Commands;

public record UpdateAlertStatusCommand(
    string AlertId,
    AlertStatus Status,
    string? PsychologistNotes = null
);
