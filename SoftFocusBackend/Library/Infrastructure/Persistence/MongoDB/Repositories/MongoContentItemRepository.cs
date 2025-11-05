using MongoDB.Bson;
using MongoDB.Driver;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Library.Infrastructure.Persistence.MongoDB.Repositories;

/// <summary>
/// Implementación MongoDB del repositorio de ContentItem
/// Incluye creación automática de índices con TTL
/// </summary>
public class MongoContentItemRepository : BaseRepository<ContentItem>, IContentItemRepository
{
    public MongoContentItemRepository(MongoDbContext context)
        : base(context, "content_items")
    {
        CreateIndexes();
    }

    public async Task<ContentItem?> FindByExternalIdAsync(string externalId)
    {
        return await Collection
            .Find(x => x.ExternalId == externalId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ContentItem>> FindByTypeAndEmotionAsync(
        ContentType contentType,
        EmotionalTag emotion,
        int limit = 20)
    {
        var matchDocument = new BsonDocument
        {
            { "ContentType", contentType.ToString() },
            { "EmotionalTags", emotion.ToString() }
        };

        var sampleDocument = new BsonDocument("size", limit);

        var pipeline = new[]
        {
            new BsonDocument("$match", matchDocument),
            new BsonDocument("$sample", sampleDocument)
        };

        return await Collection.Aggregate<ContentItem>(pipeline).ToListAsync();
    }

    public async Task<IEnumerable<ContentItem>> FindByTypeAsync(
        ContentType contentType,
        int limit = 20)
    {
        var matchDocument = new BsonDocument
        {
            { "ContentType", contentType.ToString() }
        };

        var sampleDocument = new BsonDocument("size", limit);

        var pipeline = new[]
        {
            new BsonDocument("$match", matchDocument),
            new BsonDocument("$sample", sampleDocument)
        };

        return await Collection.Aggregate<ContentItem>(pipeline).ToListAsync();
    }

    public async Task<IEnumerable<ContentItem>> FindNonExpiredAsync(int limit = 100)
    {
        var now = DateTime.UtcNow;
        return await Collection
            .Find(x => x.ExpiresAt > now)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<bool> ExistsByExternalIdAsync(string externalId)
    {
        var count = await Collection
            .CountDocumentsAsync(x => x.ExternalId == externalId);
        return count > 0;
    }

    public async Task<int> RemoveExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var result = await Collection.DeleteManyAsync(x => x.ExpiresAt <= now);
        return (int)result.DeletedCount;
    }

    /// <summary>
    /// Crea índices necesarios para ContentItem
    /// </summary>
    private void CreateIndexes()
    {
        try
        {
            // Índice único en externalId
            var externalIdIndexModel = new CreateIndexModel<ContentItem>(
                Builders<ContentItem>.IndexKeys.Ascending(x => x.ExternalId),
                new CreateIndexOptions { Unique = true }
            );

            // Índice compuesto para búsquedas por tipo y emoción
            var typeEmotionIndexModel = new CreateIndexModel<ContentItem>(
                Builders<ContentItem>.IndexKeys
                    .Ascending(x => x.ContentType)
                    .Ascending(x => x.EmotionalTags)
            );

            // Índice TTL en expiresAt (MongoDB eliminará automáticamente documentos expirados)
            var ttlIndexModel = new CreateIndexModel<ContentItem>(
                Builders<ContentItem>.IndexKeys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }
            );

            Collection.Indexes.CreateMany(new[]
            {
                externalIdIndexModel,
                typeEmotionIndexModel,
                ttlIndexModel
            });
        }
        catch
        {
            // Los índices ya pueden existir, ignorar error
        }
    }
}
