using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Infrastructure.Persistence.MongoDB.Repositories
{
    public class MongoTherapeuticRelationshipRepository : BaseRepository<TherapeuticRelationship>, ITherapeuticRelationshipRepository
    {
        public MongoTherapeuticRelationshipRepository(MongoDbContext context) 
            : base(context, "therapeutic_relationships")
        {
            CreateIndexes();
        }

        public Task<TherapeuticRelationship?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<TherapeuticRelationship?> GetByConnectionCodeAsync(ConnectionCode code)
        {
            return await Collection.Find(x => x.ConnectionCode.Value == code.Value).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TherapeuticRelationship>> GetByPsychologistIdAsync(string psychologistId)
        {
            return await Collection.Find(x => x.PsychologistId == psychologistId).ToListAsync();
        }

        public async Task<IEnumerable<TherapeuticRelationship>> GetByPatientIdAsync(string patientId)
        {
            return await Collection.Find(x => x.PatientId == patientId).ToListAsync();
        }

        public Task UpdateAsync(TherapeuticRelationship relationship)
        {
            throw new NotImplementedException();
        }

        private void CreateIndexes()
        {
            Collection.Indexes.CreateOne(new CreateIndexModel<TherapeuticRelationship>(
                Builders<TherapeuticRelationship>.IndexKeys
                    .Ascending(x => x.PsychologistId)
                    .Ascending(x => x.Status)
            ));

            Collection.Indexes.CreateOne(new CreateIndexModel<TherapeuticRelationship>(
                Builders<TherapeuticRelationship>.IndexKeys.Ascending(x => x.ConnectionCode.Value),
                new CreateIndexOptions { Unique = true }
            ));
        }
    }
}