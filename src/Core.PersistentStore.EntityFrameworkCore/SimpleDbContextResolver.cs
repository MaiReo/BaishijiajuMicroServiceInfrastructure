using Core.Session;
using Microsoft.EntityFrameworkCore;

namespace Core.PersistentStore
{
    public class SimpleDbContextResolver<TDbContext> : IDbContextResolver<TDbContext> where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private bool _sessionProviderSetted;

        public SimpleDbContextResolver(TDbContext dbContext) => this._dbContext = dbContext;

        public virtual ICoreSessionProvider SessionProvider { get; set; }

        public TDbContext GetDbContext()
        {
            if (_sessionProviderSetted)
            {
                return _dbContext;
            }
            if (SessionProvider != null && _dbContext is ICoreSessionProviderRequired sessionProviderRequired)
            {
                sessionProviderRequired.SessionProvider = SessionProvider;
                _sessionProviderSetted = true;
            }
            return _dbContext;
        }
    }
}
