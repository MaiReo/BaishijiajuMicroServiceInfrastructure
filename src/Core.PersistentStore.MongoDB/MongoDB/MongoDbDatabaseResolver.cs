using Core.PersistentStore;
using MongoDB.Driver;

namespace Core.DualCall
{
    public class MongoDbDatabaseResolver : IMongoDbDatabaseResolver
    {
        private readonly DatabaseConfiguration _databaseConfiguration;

        public MongoDbDatabaseResolver(DatabaseConfiguration databaseConfiguration)
        {
            _databaseConfiguration = databaseConfiguration;
        }
        private IMongoDatabase _database;
        public IMongoDatabase Database => _database ?? (_database = new MongoClient(_databaseConfiguration.ConnectionString).GetDatabase(_databaseConfiguration.DatabaseName));
    }
}