namespace Core.PersistentStore
{
    public class DatabaseConfiguration
    {

        public DatabaseConfiguration(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
