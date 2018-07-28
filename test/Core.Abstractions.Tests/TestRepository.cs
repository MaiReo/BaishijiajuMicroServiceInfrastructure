using Core.PersistentStore;
using Core.PersistentStore.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Abstractions.Tests
{
    internal class TestRepository<TEntity, TKey> : EFAsyncRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        public TestRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
