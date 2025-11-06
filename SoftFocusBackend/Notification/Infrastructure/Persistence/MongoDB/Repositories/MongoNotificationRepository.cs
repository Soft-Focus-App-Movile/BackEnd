using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Notification.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Model.ValueObjects;

// Alias para evitar conflicto de nombres
using NotificationAggregate = SoftFocusBackend.Notification.Domain.Model.Aggregates.Notification;

namespace SoftFocusBackend.Notification.Infrastructure.Persistence.MongoDB.Repositories;

public class MongoNotificationRepository : BaseRepository<NotificationAggregate>, INotificationRepository
{
    public MongoNotificationRepository(MongoDbContext context) : base(context, "notifications")
    {
        // Crear índices
        var indexKeys = Builders<NotificationAggregate>.IndexKeys;
        var indexes = new[]
        {
            new CreateIndexModel<NotificationAggregate>(indexKeys.Ascending(n => n.UserId)),
            new CreateIndexModel<NotificationAggregate>(indexKeys.Ascending(n => n.Type)),
            new CreateIndexModel<NotificationAggregate>(indexKeys.Ascending(n => n.Status)),
            new CreateIndexModel<NotificationAggregate>(indexKeys.Ascending(n => n.ScheduledAt)),
            new CreateIndexModel<NotificationAggregate>(indexKeys.Combine(
                indexKeys.Ascending(n => n.UserId),
                indexKeys.Descending(n => n.CreatedAt)
            ))
        };

        Collection.Indexes.CreateManyAsync(indexes);
    }

    public async Task<IEnumerable<NotificationAggregate>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.UserId, userId);
        return await Collection
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationAggregate>> GetUnreadByUserIdAsync(string userId)
    {
        var filter = Builders<NotificationAggregate>.Filter.And(
            Builders<NotificationAggregate>.Filter.Eq(n => n.UserId, userId),
            Builders<NotificationAggregate>.Filter.Eq(n => n.ReadAt, null),
            Builders<NotificationAggregate>.Filter.Ne(n => n.Status, DeliveryStatus.Read.ToString()) // ← CORRECCIÓN
        );

        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<NotificationAggregate>> GetPendingNotificationsAsync()
    {
        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Status, DeliveryStatus.Pending.ToString());
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<NotificationAggregate>> GetScheduledNotificationsAsync(DateTime currentTime)
    {
        var filter = Builders<NotificationAggregate>.Filter.And(
            Builders<NotificationAggregate>.Filter.Eq(n => n.Status, DeliveryStatus.Pending.ToString()),
            Builders<NotificationAggregate>.Filter.Lte(n => n.ScheduledAt, currentTime)
        );

        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var filter = Builders<NotificationAggregate>.Filter.And(
            Builders<NotificationAggregate>.Filter.Eq(n => n.UserId, userId),
            Builders<NotificationAggregate>.Filter.Eq(n => n.ReadAt, null),
            Builders<NotificationAggregate>.Filter.Ne(n => n.Status, DeliveryStatus.Read.ToString()) // ← CORRECCIÓN
        );

        return (int)await Collection.CountDocumentsAsync(filter);
    }

    public async Task CreateAsync(NotificationAggregate notification)
    {
        await Collection.InsertOneAsync(notification);
    }

    public async Task UpdateAsync(string notificationId, NotificationAggregate notification)
    {
        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Id, notificationId);
        await Collection.ReplaceOneAsync(filter, notification);
    }

    public async Task<NotificationAggregate> GetByIdAsync(string notificationId)
    {
        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Id, notificationId);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task DeleteAsync(string id)
    {
        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Id, id);
        await Collection.DeleteOneAsync(filter);
    }
}
