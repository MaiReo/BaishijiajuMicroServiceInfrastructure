using Core.Extensions;
using Core.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Core.PersistentStore
{
    /// <summary>
    /// 通知服务-内部通知服务
    /// </summary>
    public abstract class CorePersistentStoreDbContext : DbContext
    {
        protected virtual ICoreSession Session { get; }

        public string CurrentCityId => Session?.City?.Id;

        private static MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(CorePersistentStoreDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

        protected CorePersistentStoreDbContext(DbContextOptions options, ICoreSession session) : base(options)
        {
            Session = session ?? NullCoreSession.Instance;
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
            /*
             * 使用IEntity, IHasCreationTime, IHasModificationTime手动实现自动更新时间
             * TODO: 使用OnModelCreating使用FluentAPI在设计时添加数据库级别赋值提升效率
             */
            if (this.ChangeTracker.HasChanges())
            {
                var entries = this.ChangeTracker.Entries().ToList();
                foreach (var entry in entries)
                {
                    if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IHasCreationTime hasCreationTimeEntity)
                    {
                        hasCreationTimeEntity.CreationTime = Clock.Now;
                    }
                    if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IHasCity hasCityEntity)
                    {
                        if (string.IsNullOrWhiteSpace(hasCityEntity.CityId))
                        {
                            hasCityEntity.CityId = Session.City.Id;
                        }
                    }
                    if (entry.State == EntityState.Added && entry.Entity is IEntityBase && entry.Entity is IHasModificationTime hasModificationTimeEntityOnAdd)
                    {
                        hasModificationTimeEntityOnAdd.LastModificationTime = null;
                    }
                    if (entry.State == EntityState.Modified && entry.Entity is IEntityBase && entry.Entity is IHasModificationTime hasModificationTimeEntityOnModifing)
                    {
                        hasModificationTimeEntityOnModifing.LastModificationTime = Clock.Now;
                    }
                    if (entry.State == EntityState.Deleted && entry.Entity is IEntityBase && entry.Entity is ISoftDelete)
                    {
                        entry.Reload();
                        if (entry.State == EntityState.Unchanged)
                        {
                            entry.State = EntityState.Modified;
                            (entry.Entity as ISoftDelete).IsDeleted = true;
                        }
                    }
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

            return expression;
        }
    }
}
