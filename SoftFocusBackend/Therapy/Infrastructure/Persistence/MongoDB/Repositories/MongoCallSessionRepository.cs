using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Infrastructure.Persistence.MongoDB.Repositories
{
    public class MongoCallSessionRepository : BaseRepository<CallSession>, ICallSessionRepository
    {
        public MongoCallSessionRepository(MongoDbContext context)
            : base(context, "call_sessions")
        {
            CreateIndexes();
        }

        public async Task<CallSession?> GetByIdAsync(string id)
        {
            return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(CallSession callSession)
        {
            callSession.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<CallSession>.Filter.Eq(c => c.Id, callSession.Id);
            await Collection.ReplaceOneAsync(filter, callSession);
        }

        public async Task<IEnumerable<CallSession>> GetByParticipantIdAsync(string userId, int page, int size)
        {
            var filter = Builders<CallSession>.Filter.ElemMatch(
                c => c.Participants, p => p.UserId == userId);

            return await Collection
                .Find(filter)
                .SortByDescending(c => c.StartedAt)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();
        }

        private void CreateIndexes()
        {
            Collection.Indexes.CreateOne(new CreateIndexModel<CallSession>(
                Builders<CallSession>.IndexKeys.Ascending("participants.user_id").Descending(x => x.StartedAt)));

            Collection.Indexes.CreateOne(new CreateIndexModel<CallSession>(
                Builders<CallSession>.IndexKeys.Ascending(x => x.ChannelName)));
        }
    }
}
