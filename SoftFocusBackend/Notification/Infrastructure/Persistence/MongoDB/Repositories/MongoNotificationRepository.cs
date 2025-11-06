using MongoDB.Bson;
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
        // Crear índices para optimizar consultas
        CreateIndexesAsync().Wait();
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
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

            await Collection.Indexes.CreateManyAsync(indexes);
        }
        catch
        {
            // Índices ya existen, ignorar
        }
    }

    // ✅ CORRECTO: MongoDB Driver hace la conversión automáticamente gracias a BsonRepresentation
    public async Task<IEnumerable<NotificationAggregate>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        // Validar que el userId tenga formato de ObjectId
        if (!ObjectId.TryParse(userId, out _))
        {
            return Enumerable.Empty<NotificationAggregate>();
        }

        // No necesitas ObjectId.Parse() - MongoDB Driver lo hace automáticamente
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
        if (!ObjectId.TryParse(userId, out _))
        {
            return Enumerable.Empty<NotificationAggregate>();
        }

        var filter = Builders<NotificationAggregate>.Filter.And(
            Builders<NotificationAggregate>.Filter.Eq(n => n.UserId, userId),
            Builders<NotificationAggregate>.Filter.Eq(n => n.ReadAt, null)
        );

        return await Collection
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
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
        if (!ObjectId.TryParse(userId, out _))
        {
            return 0;
        }

        var filter = Builders<NotificationAggregate>.Filter.And(
            Builders<NotificationAggregate>.Filter.Eq(n => n.UserId, userId),
            Builders<NotificationAggregate>.Filter.Eq(n => n.ReadAt, null)
        );

        return (int)await Collection.CountDocumentsAsync(filter);
    }

    public async Task CreateAsync(NotificationAggregate notification)
    {
        // Asegurar que tenga un ID único si no lo tiene
        if (string.IsNullOrEmpty(notification.Id))
        {
            notification.Id = ObjectId.GenerateNewId().ToString();
        }

        // Asegurar que tenga timestamps
        if (notification.CreatedAt == default)
        {
            notification.CreatedAt = DateTime.UtcNow;
        }

        await Collection.InsertOneAsync(notification);
    }

    public async Task UpdateAsync(string notificationId, NotificationAggregate notification)
    {
        if (!ObjectId.TryParse(notificationId, out _))
        {
            throw new ArgumentException("Invalid notification ID format", nameof(notificationId));
        }

        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Id, notificationId);
        
        // Actualizar timestamp
        notification.UpdatedAt = DateTime.UtcNow;
        
        await Collection.ReplaceOneAsync(filter, notification);
    }

    public async Task<NotificationAggregate> GetByIdAsync(string notificationId)
    {
        if (!ObjectId.TryParse(notificationId, out _))
        {
            return null;
        }

        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Id, notificationId);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }
    
    public async Task DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            throw new ArgumentException("Invalid notification ID format", nameof(id));
        }

        var filter = Builders<NotificationAggregate>.Filter.Eq(n => n.Id, id);
        await Collection.DeleteOneAsync(filter);
    }
}
