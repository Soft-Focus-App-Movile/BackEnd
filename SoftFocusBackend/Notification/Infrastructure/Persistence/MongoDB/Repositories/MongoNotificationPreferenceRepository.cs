using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using SoftFocusBackend.Notification.Domain.Repositories;

namespace SoftFocusBackend.Notification.Infrastructure.Persistence.MongoDB.Repositories;

public class MongoNotificationPreferenceRepository : BaseRepository<NotificationPreference>, INotificationPreferenceRepository
{
    public MongoNotificationPreferenceRepository(MongoDbContext context) : base(context, "notification_preferences") // 🆕 Cambié el nombre de la colección
    {
        // Create indexes
        var indexKeys = Builders<NotificationPreference>.IndexKeys;
        var indexes = new[]
        {
            new CreateIndexModel<NotificationPreference>(indexKeys.Ascending(p => p.UserId)),
            new CreateIndexModel<NotificationPreference>(indexKeys.Combine(
                indexKeys.Ascending(p => p.UserId),
                indexKeys.Ascending(p => p.NotificationType)
            ))
        };
        
        Collection.Indexes.CreateManyAsync(indexes);
    }

    public async Task<IEnumerable<NotificationPreference>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<NotificationPreference>.Filter.Eq(p => p.UserId, userId);
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<NotificationPreference?> GetByUserAndTypeAsync(string userId, string notificationType)
    {
        var filter = Builders<NotificationPreference>.Filter.And(
            Builders<NotificationPreference>.Filter.Eq(p => p.UserId, userId),
            Builders<NotificationPreference>.Filter.Eq(p => p.NotificationType, notificationType)
        );
        
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    // 🆕 IMPLEMENTAR LOS MÉTODOS FALTANTES

    public async Task CreateAsync(NotificationPreference preference)
    {
        await Collection.InsertOneAsync(preference);
    }

    public async Task UpdateAsync(string preferenceId, NotificationPreference preference)
    {
        var filter = Builders<NotificationPreference>.Filter.Eq(p => p.Id, preferenceId);
        await Collection.ReplaceOneAsync(filter, preference);
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<NotificationPreference>.Filter.Eq(p => p.Id, id);
        await Collection.DeleteOneAsync(filter);
    }
}
