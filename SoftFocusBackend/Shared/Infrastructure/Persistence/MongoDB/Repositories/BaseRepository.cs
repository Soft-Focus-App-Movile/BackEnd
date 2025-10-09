using MongoDB.Driver;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;

namespace SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly IMongoCollection<TEntity> Collection;

    protected BaseRepository(MongoDbContext context, string collectionName)
    {
        Collection = context.GetCollection<TEntity>(collectionName);
    }

    protected BaseRepository(MongoDbContext context)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(entity);
    }

    public async Task<TEntity?> FindByIdAsync(string id)
    {
        return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public void Update(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Collection.ReplaceOne(x => x.Id == entity.Id, entity);
    }

    public void Remove(TEntity entity)
    {
        Collection.DeleteOne(x => x.Id == entity.Id);
    }

    public async Task<IEnumerable<TEntity>> ListAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}