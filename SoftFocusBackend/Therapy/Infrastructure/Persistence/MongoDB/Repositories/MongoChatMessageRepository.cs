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

        public async Task<ChatMessage?> GetByIdAsync(string id)
        {
            return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
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
        
        public async Task<ChatMessage?> GetLastMessageByReceiverIdAsync(string receiverId)
        {
            return await Collection.Find(x => x.ReceiverId == receiverId)
                .SortByDescending(x => x.Timestamp)
                .Limit(1)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(ChatMessage message)
        {
            message.UpdatedAt = DateTime.UtcNow; // Asegúrate de actualizar la fecha
            var filter = Builders<ChatMessage>.Filter.Eq(x => x.Id, message.Id);
            await Collection.ReplaceOneAsync(filter, message);
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