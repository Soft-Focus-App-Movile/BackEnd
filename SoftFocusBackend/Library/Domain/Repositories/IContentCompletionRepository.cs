using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar el historial de finalizaciones de contenido
/// </summary>
public interface IContentCompletionRepository : IBaseRepository<ContentCompletion>
{
    /// <summary>
    /// Obtiene todas las finalizaciones de un paciente
    /// </summary>
    Task<IEnumerable<ContentCompletion>> FindByPatientIdAsync(string patientId);

    /// <summary>
    /// Busca una finalización por ID de asignación
    /// </summary>
    Task<ContentCompletion?> FindByAssignmentIdAsync(string assignmentId);

    /// <summary>
    /// Verifica si una asignación ya fue completada
    /// </summary>
    Task<bool> ExistsByAssignmentIdAsync(string assignmentId);

    /// <summary>
    /// Cuenta las finalizaciones de un paciente
    /// </summary>
    Task<int> CountByPatientIdAsync(string patientId);
}
