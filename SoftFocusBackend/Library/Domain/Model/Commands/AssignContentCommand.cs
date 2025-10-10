using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Model.Commands;

/// <summary>
/// Command para asignar contenido de un psicólogo a uno o múltiples pacientes
/// </summary>
public class AssignContentCommand
{
    /// <summary>
    /// ID del psicólogo que asigna (obtenido del token JWT)
    /// </summary>
    public string PsychologistId { get; set; } = string.Empty;

    /// <summary>
    /// Lista de IDs de pacientes que recibirán la asignación
    /// </summary>
    public List<string> PatientIds { get; set; } = new();

    /// <summary>
    /// ExternalId del contenido a asignar
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// Notas o instrucciones del psicólogo
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public AssignContentCommand() { }

    /// <summary>
    /// Crea un nuevo comando
    /// </summary>
    public AssignContentCommand(
        string psychologistId,
        List<string> patientIds,
        string contentId,
        ContentType contentType,
        string notes)
    {
        PsychologistId = psychologistId;
        PatientIds = patientIds;
        ContentId = contentId;
        ContentType = contentType;
        Notes = notes;
    }

    /// <summary>
    /// Valida que el comando tenga datos válidos
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(PsychologistId))
            throw new ArgumentException("PsychologistId no puede estar vacío");

        if (PatientIds == null || PatientIds.Count == 0)
            throw new ArgumentException("Debe especificar al menos un paciente");

        if (PatientIds.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Todos los PatientIds deben ser válidos");

        if (string.IsNullOrWhiteSpace(ContentId))
            throw new ArgumentException("ContentId no puede estar vacío");
    }
}
