﻿using Core.Extensions;
using Core.PersistentStore.Auditing.Extensions;
using Core.PersistentStore.Uow;
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
    public abstract class CorePersistentStoreDbContext : DbContext, ICoreSessionProviderRequired, ICurrentUnitOfWorkRequired
    {
        public virtual ICoreSessionProvider SessionProvider { get; set; }

        public virtual ICurrentUnitOfWork CurrentUnitOfWork { get; set; }

        protected virtual string CurrentCityId => SessionProvider?.Session?.City?.Id;

        protected virtual Guid? CurrentCompanyId => SessionProvider?.Session?.Company?.Id;

        protected virtual string CurrentCompanyName => SessionProvider?.Session?.Company?.Name;

        protected virtual Guid? CurrentStoreId => SessionProvider?.Session?.Store?.Id;

        protected virtual string CurrentStoreName => SessionProvider?.Session?.Store?.Name;

        protected virtual string CurrentBrokerId => SessionProvider?.Session?.Broker?.Id;

        protected virtual string CurrentBrokerName => SessionProvider?.Session?.Broker?.Name;

        protected virtual bool IsSoftDeleteFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.SoftDelete) == true;
        protected virtual bool IsMayHaveCityFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MayHaveCity) == true;
        protected virtual bool IsMustHaveCityFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveCity) == true;
        protected virtual bool IsMayHaveCompanyFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MayHaveCompany) == true;
        protected virtual bool IsMustHaveCompanyFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveCompany) == true;
        protected virtual bool IsMayHaveStoreFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MayHaveStore) == true;
        protected virtual bool IsMustHaveStoreFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveStore) == true;
        protected virtual bool IsMustHaveBrokerFilterEnabled => CurrentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveBroker) == true;


        private static readonly MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(CorePersistentStoreDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

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
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMayHaveCompanyId haveCompanyIdEntity)
            {
                if (haveCompanyIdEntity.CompanyId == default)
                {
                    haveCompanyIdEntity.CompanyId = CurrentCompanyId;
                }
            }

            if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IMayHaveCompany haveCompanyEntity)
            {
                if (string.IsNullOrWhiteSpace(haveCompanyEntity.CompanyName))
                {
                    haveCompanyEntity.CompanyName = CurrentCompanyName;
                }
            }
        }

        private void PerformMustHaveCompany(EntityEntry entry)
        {
            if (entry.State == EntityState.Added && entry.Entity is IEntityBase)
            {
                if (entry.Entity is IMustHaveCompanyId mustHaveCompanyIdEntity)
                {
                    if (mustHaveCompanyIdEntity.CompanyId == default)
                    {
                        mustHaveCompanyIdEntity.CompanyId = CurrentCompanyId.GetValueOrDefault();
                    }
                    if (mustHaveCompanyIdEntity.CompanyId == default)
                    {
                        throw new CompanyRequiredException(entry.Entity.GetType(), "CompanyId Required but not provided.");
                    }
                }

                if (entry.Entity is IMustHaveCompany mustHaveCompanyEntity)
                {
                    if (string.IsNullOrWhiteSpace(mustHaveCompanyEntity.CompanyName))
                    {
                        mustHaveCompanyEntity.CompanyName = CurrentCompanyName;
                    }
                    if (string.IsNullOrWhiteSpace(mustHaveCompanyEntity.CompanyName))
                    {
                        throw new CompanyRequiredException(entry.Entity.GetType(), "CompanyName Required but not provided.");
                    }
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
                if (entity.StoreId == default)
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
                if (string.IsNullOrWhiteSpace(entity.StoreName))
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
            if (entityType.BaseType == null && ShouldDoFilteringEntity<TEntity>(entityType))
            {
                var filterExpression = CreateFilterExpression<TEntity>();
                if (filterExpression != null)
                {
                    modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
                }
            }
        }

        protected virtual bool ShouldDoFilteringEntity<TEntity>(IMutableEntityType entityType) where TEntity : class
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
            if (typeof(IMayHaveCompanyId).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMayHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }
            if (typeof(IMustHaveCompanyId).IsAssignableFrom(typeof(TEntity)))
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
                Expression<Func<TEntity, bool>> filter = e => !((ISoftDelete)e).IsDeleted || ((ISoftDelete)e).IsDeleted != IsSoftDeleteFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMayHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                /* This condition should normally be defined as below:
                 * CurrentCityId == null || ((IHasCity)e).CityId == CurrentCityId
                 * But this causes a problem with EF Core (see https://github.com/aspnet/EntityFrameworkCore/issues/9502)
                 * So, we made a workaround to make it working. It works same as above.
                 */
                Expression<Func<TEntity, bool>> filter = e => ((IMayHaveCity)e).CityId == CurrentCityId || (((IMayHaveCity)e).CityId != CurrentCityId && CurrentCityId == null) || (((IMayHaveCity)e).CityId == CurrentCityId) == IsMayHaveCityFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMustHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveCity)e).CityId == CurrentCityId || (((IMustHaveCity)e).CityId != CurrentCityId && CurrentCityId == null) || (((IMustHaveCity)e).CityId == CurrentCityId) == IsMustHaveCityFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMayHaveCompanyId).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMayHaveCompanyId)e).CompanyId == CurrentCompanyId || (((IMayHaveCompanyId)e).CompanyId != CurrentCompanyId && CurrentCompanyId == null) || (((IMayHaveCompany)e).CompanyId == CurrentCompanyId) == IsMayHaveCompanyFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }
            if (typeof(IMustHaveCompanyId).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveCompanyId)e).CompanyId == CurrentCompanyId || (((IMustHaveCompanyId)e).CompanyId != CurrentCompanyId && CurrentCompanyId == null) || (((IMustHaveCompany)e).CompanyId == CurrentCompanyId) == IsMustHaveCompanyFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMayHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMayHaveStore)e).StoreId == CurrentStoreId || (((IMayHaveStore)e).StoreId != CurrentStoreId && CurrentStoreId == null) || (((IMayHaveStore)e).StoreId == CurrentStoreId) == IsMayHaveStoreFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMustHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveStore)e).StoreId == CurrentStoreId || (((IMustHaveStore)e).StoreId != CurrentStoreId && CurrentStoreId == null) || (((IMustHaveStore)e).StoreId == CurrentStoreId)==IsMustHaveStoreFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            if (typeof(IMustHaveBroker).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> filter = e => ((IMustHaveBroker)e).BrokerId == CurrentBrokerId || (((IMustHaveBroker)e).BrokerId != CurrentBrokerId && CurrentBrokerId == null) || (((IMustHaveBroker)e).BrokerId == CurrentBrokerId) == IsMustHaveBrokerFilterEnabled;
                expression = expression.AndAlsoOrDefault(filter);
            }

            return expression;
        }
    }
}
