using MongoDB.Driver;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Library.Infrastructure.Persistence.MongoDB.Repositories;

/// <summary>
/// Implementación MongoDB del repositorio de ContentAssignment
/// </summary>
public class MongoContentAssignmentRepository : BaseRepository<ContentAssignment>, IContentAssignmentRepository
{
    public MongoContentAssignmentRepository(MongoDbContext context)
        : base(context, "content_assignments")
    {
        CreateIndexes();
    }

    public async Task<IEnumerable<ContentAssignment>> FindByPatientIdAsync(string patientId)
    {
        return await Collection
            .Find(x => x.PatientId == patientId)
            .SortByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContentAssignment>> FindPendingByPatientIdAsync(string patientId)
    {
        return await Collection
            .Find(x => x.PatientId == patientId && !x.IsCompleted)
            .SortByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContentAssignment>> FindCompletedByPatientIdAsync(string patientId)
    {
        return await Collection
            .Find(x => x.PatientId == patientId && x.IsCompleted)
            .SortByDescending(x => x.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContentAssignment>> FindByPsychologistIdAsync(string psychologistId)
    {
        return await Collection
            .Find(x => x.PsychologistId == psychologistId)
            .SortByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContentAssignment>> FindByPsychologistAndPatientAsync(
        string psychologistId,
        string patientId)
    {
        return await Collection
            .Find(x => x.PsychologistId == psychologistId && x.PatientId == patientId)
            .SortByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<ContentAssignment?> FindByIdAndPatientAsync(
        string assignmentId,
        string patientId)
    {
        return await Collection
            .Find(x => x.Id == assignmentId && x.PatientId == patientId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountPendingByPatientIdAsync(string patientId)
    {
        return (int)await Collection
            .CountDocumentsAsync(x => x.PatientId == patientId && !x.IsCompleted);
    }

    public async Task<int> CountCompletedByPatientIdAsync(string patientId)
    {
        return (int)await Collection
            .CountDocumentsAsync(x => x.PatientId == patientId && x.IsCompleted);
    }

    public async Task<int> CountByPsychologistIdAsync(string psychologistId)
    {
        return (int)await Collection.CountDocumentsAsync(x => x.PsychologistId == psychologistId);
    }

    /// <summary>
    /// Crea índices necesarios para ContentAssignment
    /// </summary>
    private void CreateIndexes()
    {
        try
        {
            // Índice compuesto para búsquedas por paciente y estado
            var patientStatusIndexModel = new CreateIndexModel<ContentAssignment>(
                Builders<ContentAssignment>.IndexKeys
                    .Ascending(x => x.PatientId)
                    .Ascending(x => x.IsCompleted)
            );

            // Índice para búsquedas por psicólogo
            var psychologistIndexModel = new CreateIndexModel<ContentAssignment>(
                Builders<ContentAssignment>.IndexKeys.Ascending(x => x.PsychologistId)
            );

            // Índice para búsquedas por ID de asignación
            var assignmentIdIndexModel = new CreateIndexModel<ContentAssignment>(
                Builders<ContentAssignment>.IndexKeys.Ascending(x => x.Id)
            );

            Collection.Indexes.CreateMany(new[]
            {
                patientStatusIndexModel,
                psychologistIndexModel,
                assignmentIdIndexModel
            });
        }
        catch
        {
            // Los índices ya pueden existir, ignorar error
        }
    }
}
