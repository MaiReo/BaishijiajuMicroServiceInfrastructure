using Autofac;
using Core.PersistentStore;
using Core.PersistentStore.Uow;
using Core.Session;
using Microsoft.EntityFrameworkCore;

namespace Core.TestBase
{
    public class UnitTestDbContextResolver<TDbContext> : IDbContextResolver<TDbContext> where TDbContext : DbContext
    {
        private readonly IComponentContext _componentContext;

        public UnitTestDbContextResolver(IComponentContext componentContext)
        {
            this._componentContext = componentContext;
        }

        public TDbContext GetDbContext()
        {
            var dbContext = _componentContext.Resolve<TDbContext>();
            if (dbContext is ICoreSessionProviderRequired sessionProviderRequired)
            {
                sessionProviderRequired.SessionProvider = _componentContext.Resolve<UnitTestCoreSessionProvider>();
            }
            if (dbContext is ICurrentUnitOfWorkRequired unitOfWorkRequired)
            {
                unitOfWorkRequired.CurrentUnitOfWork = _componentContext.Resolve<ICurrentUnitOfWork>();
            }
            return dbContext;
        }
    }
}
