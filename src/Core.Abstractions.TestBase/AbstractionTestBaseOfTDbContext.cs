using Autofac;
using Core.PersistentStore;
using Core.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Core.TestBase
{
    public class AbstractionTestBase<TStartup, TDbContext> : AbstractionTestBase<TStartup>
        where TStartup : class
        where TDbContext : CorePersistentStoreDbContext
    {
        protected internal override ContainerBuilder RegisterRequiredServices(IServiceCollection services)
        {
            services.AddEntityFrameworkInMemoryDatabase();
            var databaseId = Guid.NewGuid().ToString();
            services.AddDbContext<TDbContext>(
                opt =>
                {
                    opt.UseInMemoryDatabase(databaseId);
                },
                contextLifetime: ServiceLifetime.Transient);

            var builder =  base.RegisterRequiredServices(services);

            builder.RegisterGeneric(typeof(UnitTestDbContextResolver<>))
                  .AsSelf()
                  .AsImplementedInterfaces()
                  .InstancePerDependency();
            return builder;
        }

        protected void UsingDbContext(Action<TDbContext> action)
        {
            UsingDbContext(null, action);
        }

        protected void UsingDbContext(string cityId, Action<TDbContext> action)
        {
            UsingDbContext(cityId, default, action);
        }

        protected void UsingDbContext(string cityId, Guid? companyId, Action<TDbContext> action)
        {
            using (Resolve<UnitTestCoreSessionProvider>().Use(cityId, companyId))
            using (var dbContext = Resolve<IDbContextResolver<TDbContext>>().GetDbContext())
            {
                action?.Invoke(dbContext);
                dbContext.SaveChanges();
            }
        }
    }
}
