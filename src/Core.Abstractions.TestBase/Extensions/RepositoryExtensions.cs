using Core.PersistentStore;
using Core.PersistentStore.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Core.Web.Tests
{
    public static class RepositoryExtensions
    {
        internal static DbContext GetDbContextValue<TEntity, TKey>(this EFAsyncRepository<TEntity, TKey> repository) where TEntity : class, IEntity<TKey>
        {
            return typeof(EFAsyncRepository<TEntity, TKey>).GetProperty("DbContext", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(repository) as DbContext;
        }

        public static DbContext GetDbContext<TEntity, TKey>(this IRepository<TEntity, TKey> repository) where TEntity : class, IEntity<TKey>
        {
            if (!(repository is EFAsyncRepository<TEntity, TKey> efRepository))
            {
                return default;
            }
            var dbContext = efRepository.GetDbContextValue();
            return dbContext;
        }
    }
}
