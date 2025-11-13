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

        public async Task<TherapeuticRelationship?> GetByIdAsync(string id)
        {
            return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
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

        public async Task UpdateAsync(TherapeuticRelationship relationship)
        {
            var filter = Builders<TherapeuticRelationship>.Filter.Eq(r => r.Id, relationship.Id);
            await Collection.ReplaceOneAsync(filter, relationship);
        }

        private void CreateIndexes()
        {
            Collection.Indexes.CreateOne(new CreateIndexModel<TherapeuticRelationship>(
                Builders<TherapeuticRelationship>.IndexKeys
                    .Ascending(x => x.PsychologistId)
                    .Ascending(x => x.Status)
            ));

            // Index for ConnectionCode without unique constraint to allow multiple patients per psychologist
            Collection.Indexes.CreateOne(new CreateIndexModel<TherapeuticRelationship>(
                Builders<TherapeuticRelationship>.IndexKeys.Ascending(x => x.ConnectionCode.Value)
            ));

            // Ensure a patient can only have one active relationship
            var indexOptions = new CreateIndexOptions<TherapeuticRelationship>
            {
                Unique = true,
                PartialFilterExpression = Builders<TherapeuticRelationship>.Filter.Eq(x => x.IsActive, true)
            };

            Collection.Indexes.CreateOne(new CreateIndexModel<TherapeuticRelationship>(
                Builders<TherapeuticRelationship>.IndexKeys
                    .Ascending(x => x.PatientId)
                    .Ascending(x => x.IsActive),
                indexOptions
            ));
        }
    }
}