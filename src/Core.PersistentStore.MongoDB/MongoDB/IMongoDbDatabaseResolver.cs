using MongoDB.Driver;

namespace Core.DualCall
{
    public interface IMongoDbDatabaseResolver
    {
        IMongoDatabase Database { get; }
    }
}