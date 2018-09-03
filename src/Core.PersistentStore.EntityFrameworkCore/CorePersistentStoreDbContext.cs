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

        protected virtual string CurrentCompanyName => SessionProvider?.Session?.Company?.Name;

        protected virtual Guid? CurrentStoreId => SessionProvider?.Session?.Store?.Id;

        protected virtual string CurrentStoreName => SessionProvider?.Session?.Store?.Name;

        protected virtual string CurrentBrokerId => SessionProvider?.Session?.Broker?.Id;

        protected virtual string CurrentBrokerName => SessionProvider?.Session?.Broker?.Name;

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
                    PerformStore(entry);
                    PerformBroker(entry);
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
            PerformMayHaveCity(entry);
            PerformMustHaveCity(entry);
        }

        private void PerformMayHaveCity(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMayHaveCity entity)
            {
                if (string.IsNullOrWhiteSpace(entity.CityId))
                {
                    entity.CityId = CurrentCityId;
                }
            }
        }

        private void PerformMustHaveCity(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMustHaveCity entity)
            {
                if (string.IsNullOrWhiteSpace(entity.CityId))
                {
                    entity.CityId = CurrentCityId;
                }
                if (string.IsNullOrWhiteSpace(entity.CityId))
                {
                    throw new CityRequiredException(entry.Entity.GetType());
                }
            }

        }

        private void PerformCompany(EntityEntry entry)
        {
            PerformMayHaveCompany(entry);
            PerformMustHaveCompany(entry);
        }

        private void PerformMayHaveCompany(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMayHaveCompany entity)
            {
                if (!entity.CompanyId.HasValue)
                {
                    entity.CompanyId = CurrentCompanyId;
                }
                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                {
                    entity.CompanyName = CurrentCompanyName;
                }
            }
        }

        private void PerformMustHaveCompany(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMustHaveCompany entity)
            {
                if (entity.CompanyId == default)
                {
                    entity.CompanyId = CurrentCompanyId.GetValueOrDefault();
                }
                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                {
                    entity.CompanyName = CurrentCompanyName;
                }
                if (entity.CompanyId == default)
                {
                    throw new CompanyRequiredException(entry.Entity.GetType(), "CompanyId Required but not provided.");
                }
                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                {
                     throw new CompanyRequiredException(entry.Entity.GetType(), "CompanyName Required but not provided.");
                }
            }
        }

        private void PerformStore(EntityEntry entry)
        {
            PerformMayHaveStore(entry);
            PerformMustHaveStore(entry);
        }
        private void PerformMayHaveStore(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMayHaveStore entity)
            {
                if (!entity.StoreId.HasValue)
                {
                    entity.StoreId = CurrentStoreId;
                }
                if (string.IsNullOrWhiteSpace(entity.StoreName))
                {
                    entity.StoreName = CurrentStoreName;
                }
            }
        }
        private void PerformMustHaveStore(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMustHaveStore entity)
            {
                if (entity.StoreId == default)
                {
                    entity.StoreId = CurrentStoreId.GetValueOrDefault();
                }
                if (!string.IsNullOrWhiteSpace(entity.StoreName))
                {
                    entity.StoreName = CurrentStoreName;
                }
                if (entity.StoreId == default)
                {
                    throw new StoreRequiredException(entry.Entity.GetType(), "StoreId Required but not provided.");
                }
                if (string.IsNullOrWhiteSpace(entity.StoreName))
                {
                    throw new StoreRequiredException(entry.Entity.GetType(), "StoreName Required but not provided.");
                }
            }
        }

        private void PerformBroker(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMustHaveBroker entity)
            {
                if (string.IsNullOrWhiteSpace(entity.BrokerId))
                {
                    entity.BrokerId = CurrentBrokerId;
                }
                if (string.IsNullOrWhiteSpace(entity.BrokerName))
                {
                    entity.BrokerName = CurrentBrokerName;
                }
                if (string.IsNullOrWhiteSpace(entity.BrokerId))
                {
                    throw new BrokerRequiredException(entry.Entity.GetType(), "BrokerId Required but not provided.");
                }
                if (string.IsNullOrWhiteSpace(entity.BrokerName))
                {
                    throw new BrokerRequiredException(entry.Entity.GetType(), "BrokerName Required but not provided.");
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

            if (typeof(IMayHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMustHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMayHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMustHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMayHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMustHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMustHaveBroker).IsAssignableFrom(typeof(TEntity)))
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
                Expression<Func<TEntity, bool>> filter = e => !((ISoftDelete)e).IsDeleted;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMayHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                /* This condition should normally be defined as below:
                 * CurrentCityId == null || ((IHasCity)e).CityId == CurrentCityId
                 * But this causes a problem with EF Core (see https://github.com/aspnet/EntityFrameworkCore/issues/9502)
                 * So, we made a workaround to make it working. It works same as above.
                 */
                Expression<Func<TEntity, bool>> filter = e => ((IMayHaveCity)e).CityId == CurrentCityId || (((IMayHaveCity)e).CityId != CurrentCityId && CurrentCityId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }
            
            if (typeof(IMustHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveCity)e).CityId == CurrentCityId || (((IMustHaveCity)e).CityId != CurrentCityId && CurrentCityId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMayHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMayHaveCompany)e).CompanyId == CurrentCompanyId || (((IMayHaveCompany)e).CompanyId != CurrentCompanyId && CurrentCompanyId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }
            if (typeof(IMustHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveCompany)e).CompanyId == CurrentCompanyId || (((IMustHaveCompany)e).CompanyId != CurrentCompanyId && CurrentCompanyId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMayHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMayHaveStore)e).StoreId == CurrentStoreId || (((IMayHaveStore)e).StoreId != CurrentStoreId && CurrentStoreId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMustHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveStore)e).StoreId == CurrentStoreId || (((IMustHaveStore)e).StoreId != CurrentStoreId && CurrentStoreId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMustHaveBroker).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveBroker)e).BrokerId == CurrentBrokerId || (((IMustHaveBroker)e).BrokerId != CurrentBrokerId && CurrentBrokerId == null);
                expression = expression.AndAlsoOrDefault(filter);
            }

            return expression;
        }
    }
}
