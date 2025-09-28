using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Configuration;

namespace SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        
        if (!_settings.IsValid)
            throw new InvalidOperationException("MongoDB settings are not properly configured");

        var clientSettings = MongoClientSettings.FromConnectionString(_settings.ConnectionString);
        clientSettings.MaxConnectionPoolSize = _settings.MaxConnectionPoolSize;
        clientSettings.ServerSelectionTimeout = _settings.ServerSelectionTimeout;
        clientSettings.SocketTimeout = _settings.SocketTimeout;

        var client = new MongoClient(clientSettings);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase Database => _database;
}