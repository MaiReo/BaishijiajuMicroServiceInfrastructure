using Autofac;
using Core.PersistentStore;
using System;
using System.Linq;
using System.Reflection;

namespace Core.DualCall
{
    internal class MongoDbModule : Autofac.Module
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDbModule(string connectionString, string databaseName)
        {
            this._connectionString = connectionString;
            this._databaseName = databaseName;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new DatabaseConfiguration(_connectionString, _databaseName));
            builder.RegisterType<MongoDbDatabaseResolver>()
                .As<IMongoDbDatabaseResolver>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
            builder.RegisterAssemblyByConvention(typeof(MongoDbModule).Assembly);
        }
    }
}
