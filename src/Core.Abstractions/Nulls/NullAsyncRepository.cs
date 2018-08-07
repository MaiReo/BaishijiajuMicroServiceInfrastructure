using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.PersistentStore.Repositories
{
    public class NullAsyncRepository<TEntity, TKey> : IAsyncRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        public T Query<T>(Func<IQueryable<TEntity>, T> predicate)
        {
            return default;
        }

        public Task<TEntity> FirstOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TEntity));
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TEntity));
        }

        public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TEntity));
        }

        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TEntity));
        }

        public Task<TEntity> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TEntity));
        }

        public Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TEntity));
        }

        public Task EnsureCollectionLoadedAsync<T>(TEntity entity, Expression<Func<TEntity, IEnumerable<T>>> collectionSelector, CancellationToken cancellationToken = default) where T : class
        {
            return Task.FromResult(default(TEntity));
        }

        public Task EnsureNavigationLoadedAsync<T>(TEntity entity, Expression<Func<TEntity, T>> propertySelector, CancellationToken cancellationToken = default) where T : class
        {
            return Task.FromResult(default(TEntity));
        }

        public IQueryable<TEntity> GetAll()
        {
            return Enumerable.Empty<TEntity>().AsQueryable();
        }
    }
}
