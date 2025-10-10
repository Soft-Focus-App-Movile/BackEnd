using MongoDB.Driver;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.AI.Infrastructure.Persistence.MongoDB.Repositories;

public class MongoChatSessionRepository : BaseRepository<ChatSession>, IChatSessionRepository
{
    private readonly IMongoCollection<ChatMessage> _messagesCollection;

    public MongoChatSessionRepository(MongoDbContext context) : base(context, "chat_sessions")
    {
        _messagesCollection = context.Database.GetCollection<ChatMessage>("chat_messages");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var sessionIndexKeys = Builders<ChatSession>.IndexKeys
            .Ascending(s => s.UserId)
            .Descending(s => s.StartedAt);
        var sessionIndexModel = new CreateIndexModel<ChatSession>(sessionIndexKeys);
        Collection.Indexes.CreateOne(sessionIndexModel);

        var activeIndexKeys = Builders<ChatSession>.IndexKeys.Ascending(s => s.IsActive);
        var activeIndexModel = new CreateIndexModel<ChatSession>(activeIndexKeys);
        Collection.Indexes.CreateOne(activeIndexModel);

        var messageSessionIndexKeys = Builders<ChatMessage>.IndexKeys
            .Ascending(m => m.SessionId)
            .Ascending(m => m.Timestamp);
        var messageIndexModel = new CreateIndexModel<ChatMessage>(messageSessionIndexKeys);
        _messagesCollection.Indexes.CreateOne(messageIndexModel);
    }

    public async Task<ChatSession?> GetByIdAsync(string sessionId)
    {
        return await Collection.Find(s => s.Id == sessionId).FirstOrDefaultAsync();
    }

    public async Task<ChatSession> CreateAsync(string userId)
    {
        var session = ChatSession.Create(userId);
        await Collection.InsertOneAsync(session);
        return session;
    }

    public async Task AddMessageAsync(string sessionId, ChatMessage message)
    {
        await _messagesCollection.InsertOneAsync(message);

        var filter = Builders<ChatSession>.Filter.Eq(s => s.Id, sessionId);
        var update = Builders<ChatSession>.Update
            .Inc(s => s.MessageCount, 1)
            .Set(s => s.LastMessageAt, DateTime.UtcNow)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);

        await Collection.UpdateOneAsync(filter, update);
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId, DateTime? from, DateTime? to, int pageSize)
    {
        var filterBuilder = Builders<ChatSession>.Filter;
        var filter = filterBuilder.Eq(s => s.UserId, userId);

        if (from.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Gte(s => s.StartedAt, from.Value));
        }

        if (to.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lte(s => s.StartedAt, to.Value));
        }

        return await Collection.Find(filter)
            .SortByDescending(s => s.StartedAt)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId, int limit = 10)
    {
        return await _messagesCollection.Find(m => m.SessionId == sessionId)
            .SortByDescending(m => m.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task UpdateAsync(ChatSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(s => s.Id == session.Id, session);
    }
}
