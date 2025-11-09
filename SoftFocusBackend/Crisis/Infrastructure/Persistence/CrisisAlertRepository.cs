using MongoDB.Driver;
using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Crisis.Infrastructure.Persistence;

public class CrisisAlertRepository : BaseRepository<CrisisAlert>, ICrisisAlertRepository
{
    public CrisisAlertRepository(MongoDbContext context) : base(context, "crisisAlerts")
    {
    }

    public async Task<IEnumerable<CrisisAlert>> FindByPsychologistIdAsync(
        string psychologistId,
        AlertSeverity? severity = null,
        AlertStatus? status = null,
        int? limit = null)
    {
        var filterBuilder = Builders<CrisisAlert>.Filter;
        var filter = filterBuilder.Eq(a => a.PsychologistId, psychologistId);

        if (severity.HasValue)
        {
            filter &= filterBuilder.Eq(a => a.Severity, severity.Value);
        }

        if (status.HasValue)
        {
            filter &= filterBuilder.Eq(a => a.Status, status.Value);
        }

        var query = Collection.Find(filter).SortByDescending(a => a.CreatedAt);

        if (limit.HasValue)
        {
            return await query.Limit(limit.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<CrisisAlert>> FindByPatientIdAsync(string patientId, int? limit = null)
    {
        var filter = Builders<CrisisAlert>.Filter.Eq(a => a.PatientId, patientId);
        var query = Collection.Find(filter).SortByDescending(a => a.CreatedAt);

        if (limit.HasValue)
        {
            return await query.Limit(limit.Value).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<CrisisAlert>> FindPendingCriticalAlertsAsync(string psychologistId)
    {
        var filter = Builders<CrisisAlert>.Filter.And(
            Builders<CrisisAlert>.Filter.Eq(a => a.PsychologistId, psychologistId),
            Builders<CrisisAlert>.Filter.Eq(a => a.Severity, AlertSeverity.Critical),
            Builders<CrisisAlert>.Filter.Eq(a => a.Status, AlertStatus.Pending)
        );

        return await Collection.Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountPendingAlertsByPsychologistAsync(string psychologistId, AlertSeverity? severity = null)
    {
        var filterBuilder = Builders<CrisisAlert>.Filter;
        var filter = filterBuilder.And(
            filterBuilder.Eq(a => a.PsychologistId, psychologistId),
            filterBuilder.Eq(a => a.Status, AlertStatus.Pending)
        );

        if (severity.HasValue)
        {
            filter &= filterBuilder.Eq(a => a.Severity, severity.Value);
        }

        return (int)await Collection.CountDocumentsAsync(filter);
    }
}
