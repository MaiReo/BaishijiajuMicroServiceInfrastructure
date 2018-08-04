using Autofac;
using Core.PersistentStore;
using Core.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Core.Abstractions.TestBase
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
                });
            return base.RegisterRequiredServices(services);
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
            var oldCity = TestSession.Instance.City;
            var oldCompany = TestSession.Instance.Company;
            try
            {
                TestSession.Instance.City = new City(cityId);
                TestSession.Instance.Company = new Company(companyId);
                var dbContext = Resolve<TDbContext>();
                action?.Invoke(dbContext);
                dbContext.SaveChanges();
                if (!UseScopedResolver)
                {
                    dbContext.Dispose();
                }
            }
            finally
            {
                TestSession.Instance.City = oldCity;
                TestSession.Instance.Company = oldCompany;
            }

        }
    }
}
