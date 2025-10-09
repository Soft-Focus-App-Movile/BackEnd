using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar las asignaciones de contenido
/// </summary>
public interface IContentAssignmentRepository : IBaseRepository<ContentAssignment>
{
    /// <summary>
    /// Obtiene todas las asignaciones de un paciente
    /// </summary>
    Task<IEnumerable<ContentAssignment>> FindByPatientIdAsync(string patientId);

    /// <summary>
    /// Obtiene asignaciones pendientes de un paciente
    /// </summary>
    Task<IEnumerable<ContentAssignment>> FindPendingByPatientIdAsync(string patientId);

    /// <summary>
    /// Obtiene asignaciones completadas de un paciente
    /// </summary>
    Task<IEnumerable<ContentAssignment>> FindCompletedByPatientIdAsync(string patientId);

    /// <summary>
    /// Obtiene todas las asignaciones creadas por un psicólogo
    /// </summary>
    Task<IEnumerable<ContentAssignment>> FindByPsychologistIdAsync(string psychologistId);

    /// <summary>
    /// Obtiene asignaciones de un psicólogo para un paciente específico
    /// </summary>
    Task<IEnumerable<ContentAssignment>> FindByPsychologistAndPatientAsync(
        string psychologistId,
        string patientId);

    /// <summary>
    /// Busca una asignación por ID y valida que pertenezca al paciente
    /// </summary>
    Task<ContentAssignment?> FindByIdAndPatientAsync(
        string assignmentId,
        string patientId);

    /// <summary>
    /// Cuenta asignaciones pendientes de un paciente
    /// </summary>
    Task<int> CountPendingByPatientIdAsync(string patientId);

    /// <summary>
    /// Cuenta asignaciones completadas de un paciente
    /// </summary>
    Task<int> CountCompletedByPatientIdAsync(string patientId);

    /// <summary>
    /// Cuenta asignaciones totales de un psicólogo
    /// </summary>
    Task<int> CountByPsychologistIdAsync(string psychologistId);
}
