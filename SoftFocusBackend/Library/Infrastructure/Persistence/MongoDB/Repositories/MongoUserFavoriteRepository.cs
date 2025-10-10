using MongoDB.Driver;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Library.Infrastructure.Persistence.MongoDB.Repositories;

/// <summary>
/// Implementación MongoDB del repositorio de UserFavorite
/// </summary>
public class MongoUserFavoriteRepository : BaseRepository<UserFavorite>, IUserFavoriteRepository
{
    public MongoUserFavoriteRepository(MongoDbContext context)
        : base(context, "user_favorites")
    {
        CreateIndexes();
    }

    public async Task<IEnumerable<UserFavorite>> FindByUserIdAsync(string userId)
    {
        return await Collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.AddedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserFavorite>> FindByUserIdAndTypeAsync(
        string userId,
        ContentType contentType)
    {
        return await Collection
            .Find(x => x.UserId == userId && x.ContentType == contentType)
            .SortByDescending(x => x.AddedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserFavorite>> FindByUserIdAndEmotionAsync(
        string userId,
        EmotionalTag emotion)
    {
        return await Collection
            .Find(x => x.UserId == userId && x.Content.EmotionalTags.Contains(emotion))
            .SortByDescending(x => x.AddedAt)
            .ToListAsync();
    }

    public async Task<UserFavorite?> FindByUserAndContentAsync(
        string userId,
        string contentId)
    {
        return await Collection
            .Find(x => x.UserId == userId && x.ContentId == contentId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(string userId, string contentId)
    {
        var count = await Collection
            .CountDocumentsAsync(x => x.UserId == userId && x.ContentId == contentId);
        return count > 0;
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        return (int)await Collection.CountDocumentsAsync(x => x.UserId == userId);
    }

    /// <summary>
    /// Crea índices necesarios para UserFavorite
    /// </summary>
    private void CreateIndexes()
    {
        try
        {
            // Índice compuesto para búsquedas por userId y contentType
            var userTypeIndexModel = new CreateIndexModel<UserFavorite>(
                Builders<UserFavorite>.IndexKeys
                    .Ascending(x => x.UserId)
                    .Ascending(x => x.ContentType)
            );

            // Índice único compuesto para evitar duplicados
            var uniqueUserContentIndexModel = new CreateIndexModel<UserFavorite>(
                Builders<UserFavorite>.IndexKeys
                    .Ascending(x => x.UserId)
                    .Ascending(x => x.ContentId),
                new CreateIndexOptions { Unique = true }
            );

            Collection.Indexes.CreateMany(new[]
            {
                userTypeIndexModel,
                uniqueUserContentIndexModel
            });
        }
        catch
        {
            // Los índices ya pueden existir, ignorar error
        }
    }
}
