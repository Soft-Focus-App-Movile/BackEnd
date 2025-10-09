using MongoDB.Driver;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Library.Infrastructure.Persistence.MongoDB.Repositories;

/// <summary>
/// Implementación MongoDB del repositorio de ContentCompletion
/// </summary>
public class MongoContentCompletionRepository : BaseRepository<ContentCompletion>, IContentCompletionRepository
{
    public MongoContentCompletionRepository(MongoDbContext context)
        : base(context, "content_completions")
    {
        CreateIndexes();
    }

    public async Task<IEnumerable<ContentCompletion>> FindByPatientIdAsync(string patientId)
    {
        return await Collection
            .Find(x => x.PatientId == patientId)
            .SortByDescending(x => x.CompletedAt)
            .ToListAsync();
    }

    public async Task<ContentCompletion?> FindByAssignmentIdAsync(string assignmentId)
    {
        return await Collection
            .Find(x => x.AssignmentId == assignmentId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByAssignmentIdAsync(string assignmentId)
    {
        var count = await Collection
            .CountDocumentsAsync(x => x.AssignmentId == assignmentId);
        return count > 0;
    }

    public async Task<int> CountByPatientIdAsync(string patientId)
    {
        return (int)await Collection.CountDocumentsAsync(x => x.PatientId == patientId);
    }

    /// <summary>
    /// Crea índices necesarios para ContentCompletion
    /// </summary>
    private void CreateIndexes()
    {
        try
        {
            // Índice para búsquedas por paciente
            var patientIndexModel = new CreateIndexModel<ContentCompletion>(
                Builders<ContentCompletion>.IndexKeys.Ascending(x => x.PatientId)
            );

            // Índice único para assignmentId (una asignación solo se puede completar una vez)
            var assignmentIndexModel = new CreateIndexModel<ContentCompletion>(
                Builders<ContentCompletion>.IndexKeys.Ascending(x => x.AssignmentId),
                new CreateIndexOptions { Unique = true }
            );

            Collection.Indexes.CreateMany(new[]
            {
                patientIndexModel,
                assignmentIndexModel
            });
        }
        catch
        {
            // Los índices ya pueden existir, ignorar error
        }
    }
}
