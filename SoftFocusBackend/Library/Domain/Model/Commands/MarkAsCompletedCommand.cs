namespace SoftFocusBackend.Library.Domain.Model.Commands;

/// <summary>
/// Command para marcar una asignación de contenido como completada
/// </summary>
public class MarkAsCompletedCommand
{
    /// <summary>
    /// ID del paciente que completa (obtenido del token JWT)
    /// </summary>
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// ID de la asignación a marcar como completada
    /// </summary>
    public string AssignmentId { get; set; } = string.Empty;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public MarkAsCompletedCommand() { }

    /// <summary>
    /// Crea un nuevo comando
    /// </summary>
    public MarkAsCompletedCommand(string patientId, string assignmentId)
    {
        PatientId = patientId;
        AssignmentId = assignmentId;
    }

    /// <summary>
    /// Valida que el comando tenga datos válidos
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(PatientId))
            throw new ArgumentException("PatientId no puede estar vacío");

        if (string.IsNullOrWhiteSpace(AssignmentId))
            throw new ArgumentException("AssignmentId no puede estar vacío");
    }
}
