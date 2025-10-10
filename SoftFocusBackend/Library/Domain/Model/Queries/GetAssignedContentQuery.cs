namespace SoftFocusBackend.Library.Domain.Model.Queries;

/// <summary>
/// Query para obtener el contenido asignado a un paciente
/// </summary>
public class GetAssignedContentQuery
{
    /// <summary>
    /// ID del paciente
    /// </summary>
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Filtrar solo pendientes (false) o solo completados (true) o todos (null)
    /// </summary>
    public bool? CompletedFilter { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public GetAssignedContentQuery() { }

    /// <summary>
    /// Crea una nueva query
    /// </summary>
    public GetAssignedContentQuery(string patientId, bool? completedFilter = null)
    {
        PatientId = patientId;
        CompletedFilter = completedFilter;
    }

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(PatientId))
            throw new ArgumentException("PatientId no puede estar vacío");
    }
}
