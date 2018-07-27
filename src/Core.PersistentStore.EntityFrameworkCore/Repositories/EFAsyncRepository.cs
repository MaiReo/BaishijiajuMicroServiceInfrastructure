using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.PersistentStore.Repositories
{
    public abstract class EFAsyncRepository<TEntity, TKey> : IAsyncRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        protected EFAsyncRepository(DbContext dbContext)
        {
            DbContext = dbContext;
        }
        protected virtual DbContext DbContext { get; }

        public virtual async Task<TEntity> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await FirstOrDefaultAsync(id, cancellationToken);
            if (entity == default)
            {
                return default;
            }
            DbContext.Entry(entity).State = EntityState.Deleted;
            await DbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await DbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var entity = await DbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
            return entity;
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }

        public virtual async Task EnsureNavigationLoadedAsync<T>(TEntity entity, Expression<Func<TEntity, T>> propertySelector, CancellationToken cancellationToken = default) where T : class
        {
            var propertyName = (propertySelector.Body as MemberExpression)?.Member?.Name;
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }
            var entry = DbContext.Entry(entity).Navigation(propertyName);
            if (entry.IsLoaded)
            {
                return;
            }
            await DbContext.Entry(entity).Navigation(propertyName).LoadAsync(cancellationToken);
        }

        public virtual async Task EnsureCollectionLoadedAsync<T>(TEntity entity, Expression<Func<TEntity, IEnumerable<T>>> collectionSelector, CancellationToken cancellationToken = default) where T : class
        {
            var entry = DbContext.Entry(entity).Collection(collectionSelector);
            if (entry.IsLoaded)
            {
                return;
            }
            await entry.LoadAsync(cancellationToken);
        }

        public virtual T Query<T>(Func<IQueryable<TEntity>, T> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return predicate(DbContext.Set<TEntity>());
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Entry(entity);
            if (entry.State != EntityState.Modified)
            {
                entry.State = EntityState.Modified;
            }
            await DbContext.SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }
    }
}
