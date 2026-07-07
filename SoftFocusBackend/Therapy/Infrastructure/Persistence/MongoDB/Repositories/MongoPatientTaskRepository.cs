using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Infrastructure.Persistence.MongoDB.Repositories
{
    public class MongoPatientTaskRepository : BaseRepository<PatientTask>, IPatientTaskRepository
    {
        public MongoPatientTaskRepository(MongoDbContext context)
            : base(context, "patient_tasks")
        {
            CreateIndexes();
        }

        public async Task<PatientTask?> GetByIdAsync(string id)
        {
            return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatientTask>> GetByPatientIdAsync(string patientId)
        {
            return await Collection.Find(x => x.PatientId == patientId)
                .SortByDescending(x => x.AssignedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PatientTask>> GetByPsychologistAndPatientAsync(string psychologistId, string patientId)
        {
            return await Collection.Find(x => x.PsychologistId == psychologistId && x.PatientId == patientId)
                .SortByDescending(x => x.AssignedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(PatientTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<PatientTask>.Filter.Eq(x => x.Id, task.Id);
            await Collection.ReplaceOneAsync(filter, task);
        }

        private void CreateIndexes()
        {
            Collection.Indexes.CreateOne(new CreateIndexModel<PatientTask>(
                Builders<PatientTask>.IndexKeys
                    .Ascending(x => x.PatientId)
                    .Descending(x => x.AssignedAt)));

            Collection.Indexes.CreateOne(new CreateIndexModel<PatientTask>(
                Builders<PatientTask>.IndexKeys.Ascending(x => x.PsychologistId)));
        }
    }
}
