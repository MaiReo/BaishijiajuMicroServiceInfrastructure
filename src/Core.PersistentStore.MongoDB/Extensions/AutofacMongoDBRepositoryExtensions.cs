using Core.DualCall;

namespace Autofac
{
    public static class AutofacMongoDbRepositoryExtensions
    {
        public static ContainerBuilder AddMongoDb(this ContainerBuilder builder, string connectionString, string databaseName)
        {
            builder.RegisterModule(new MongoDbModule(connectionString, databaseName));
            return builder;
        }
    }
}
