using Core.Extensions;
using Core.PersistentStore.Auditing.Extensions;
using Core.Session;
using Core.Session.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Core.PersistentStore
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CorePersistentStoreDbContext : DbContext, ICoreSessionProviderRequired
    {
        public virtual ICoreSessionProvider SessionProvider { get; set; }

        protected virtual string CurrentCityId => SessionProvider?.Session?.City?.Id;

        protected virtual Guid? CurrentCompanyId => SessionProvider?.Session?.Company?.Id;

        private static MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(CorePersistentStoreDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

        public CorePersistentStoreDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// See <see cref="DbContext.SaveChanges(bool)"/>
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnSavingChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        /// <summary>
        /// See <see cref="DbContext.SaveChangesAsync(bool, CancellationToken)"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnSavingChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnSavingChanges()
        {
            if (ChangeTracker.HasChanges())
            {
                var entries = ChangeTracker.Entries().ToList();
                foreach (var entry in entries)
                {
                    PerformCity(entry);
                    PerformCompany(entry);
                    PerformAuditing(entry);
                }
            }
        }


        private void PerformAuditing(EntityEntry entry)
        {
            var currentUserId = SessionProvider?.Session?.GetCurrentUserId();
            var currentUserName = SessionProvider?.Session?.GetCurrentUserName();
            entry.PerformAuditing(currentUserId, currentUserName);
        }

        private void PerformCity(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IHasCity hasCityEntity)
            {
                if (string.IsNullOrWhiteSpace(hasCityEntity.CityId))
                {
                    hasCityEntity.CityId = CurrentCityId;
                }
            }
        }

        private void PerformCompany(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMayHaveCompany mayHaveCompanyEntity)
            {
                if (!mayHaveCompanyEntity.BrokerCompanyId.HasValue)
                {
                    mayHaveCompanyEntity.BrokerCompanyId = CurrentCompanyId;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                ConfigureGlobalFiltersMethodInfo
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType });
            }
        }

        protected void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType)
            where TEntity : class
        {
            if (entityType.BaseType == null && ShouldFilterEntity<TEntity>(entityType))
            {
                var filterExpression = CreateFilterExpression<TEntity>();
                if (filterExpression != null)
                {
                    modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
                }
            }
        }

        protected virtual bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType) where TEntity : class
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }

            if (typeof(IHasCity).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMayHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            return false;
        }

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
            where TEntity : class
        {
            Expression<Func<TEntity, bool>> expression = null;

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> softDeleteFilter = e => !((ISoftDelete)e).IsDeleted;
                expression = expression.AndAlsoOrDefault(softDeleteFilter);
            }

            if (typeof(IHasCity).IsAssignableFrom(typeof(TEntity)))
            {
                /* This condition should normally be defined as below:
                 * CurrentCityId == null || ((IHasCity)e).CityId == CurrentCityId
                 * But this causes a problem with EF Core (see https://github.com/aspnet/EntityFrameworkCore/issues/9502)
                 * So, we made a workaround to make it working. It works same as above.
                 */
                Expression<Func<TEntity, bool>> hasCityFilter = e => ((IHasCity)e).CityId == CurrentCityId || (((IHasCity)e).CityId != CurrentCityId && CurrentCityId == null);
                expression = expression.AndAlsoOrDefault(hasCityFilter);
            }

            if (typeof(IMayHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> mayHaveCompanyFilter = e => ((IMayHaveCompany)e).BrokerCompanyId == CurrentCompanyId;
                expression = expression.AndAlsoOrDefault(mayHaveCompanyFilter);
            }

            return expression;
        }
    }
}
