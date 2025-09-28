namespace SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int MaxConnectionPoolSize { get; set; }
    public int ServerSelectionTimeoutSeconds { get; set; }
    public int SocketTimeoutSeconds { get; set; }

    public TimeSpan ServerSelectionTimeout => TimeSpan.FromSeconds(ServerSelectionTimeoutSeconds);
    public TimeSpan SocketTimeout => TimeSpan.FromSeconds(SocketTimeoutSeconds);

    public bool IsValid => 
        !string.IsNullOrEmpty(ConnectionString) && 
        !string.IsNullOrEmpty(DatabaseName);
}