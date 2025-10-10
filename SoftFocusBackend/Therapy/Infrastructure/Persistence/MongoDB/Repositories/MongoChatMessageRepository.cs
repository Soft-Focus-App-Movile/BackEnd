using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Infrastructure.Persistence.MongoDB.Repositories
{
    public class MongoChatMessageRepository : BaseRepository<ChatMessage>, IChatMessageRepository
    {
        public MongoChatMessageRepository(MongoDbContext context) 
            : base(context, "chat_messages")
        {
            CreateIndexes();
        }

        public Task<ChatMessage?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ChatMessage>> GetByRelationshipIdAsync(string relationshipId, int page, int size)
        {
            return await Collection.Find(x => x.RelationshipId == relationshipId)
                .SortByDescending(x => x.Timestamp)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetUnreadByReceiverIdAsync(string receiverId)
        {
            return await Collection.Find(x => x.ReceiverId == receiverId && !x.IsRead)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public Task UpdateAsync(ChatMessage message)
        {
            throw new NotImplementedException();
        }

        private void CreateIndexes()
        {
            Collection.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys
                    .Ascending(x => x.RelationshipId)
                    .Descending(x => x.Timestamp)
            ));

            Collection.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys.Ascending(x => x.ReceiverId)
            ));
        }
    }
}