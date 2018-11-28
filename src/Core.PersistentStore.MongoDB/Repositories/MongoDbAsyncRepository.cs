using Core.DualCall.Extensions;
using Core.PersistentStore;
using Core.PersistentStore.Exceptions;
using Core.PersistentStore.Repositories;
using Core.PersistentStore.Uow;
using Core.Session;
using MongoDB.Driver;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.DualCall.Repositories
{
    public class MongoDbAsyncRepository<TEntity> : MongoDbAsyncRepository<TEntity, int>, IAsyncRepository<TEntity, int>, IRepository<TEntity, int> where TEntity : class, IEntity
    {
        public MongoDbAsyncRepository(
            IMongoDbDatabaseResolver databaseResolver,
            ICurrentUnitOfWork currentUnitOfWork = null,
            ICoreSessionProvider coreSessionProvider = null) : base(databaseResolver, currentUnitOfWork, coreSessionProvider)
        {
        }
    }

    public class MongoDbAsyncRepository<TEntity, TKey> : AsyncCommonRepositoryBase<TEntity, TKey>, IAsyncRepository<TEntity, TKey>, IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoDbDatabaseResolver _databaseResolver;
        private readonly ICurrentUnitOfWork _currentUnitOfWork;
        private readonly ICoreSessionProvider _coreSessionProvider;
        private readonly string _collectionName;
        private readonly Func<TEntity, bool> _defaultValueEqualityDelegate;

        public MongoDbAsyncRepository(
            IMongoDbDatabaseResolver databaseResolver,
            ICurrentUnitOfWork currentUnitOfWork = null,
            ICoreSessionProvider coreSessionProvider = null)
        {
            _databaseResolver = databaseResolver;
            _currentUnitOfWork = currentUnitOfWork;
            _coreSessionProvider = coreSessionProvider;
            var entityType = typeof(TEntity);
            _collectionName = entityType.GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault()?.Name ?? entityType.Name;
            _defaultValueEqualityDelegate = CreateDefaultValueEqualityExpressionForId().Compile();

        }

        protected virtual bool IsSoftDeleteFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.SoftDelete) == true;
        protected virtual bool IsMayHaveCityFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.MayHaveCity) == true;
        protected virtual bool IsMustHaveCityFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveCity) == true;
        protected virtual bool IsMayHaveCompanyFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.MayHaveCompany) == true;
        protected virtual bool IsMustHaveCompanyFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveCompany) == true;
        protected virtual bool IsMayHaveStoreFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.MaytHaveStore) == true;
        protected virtual bool IsMustHaveStoreFilterEnabled => _currentUnitOfWork?.IsFilterEnabled(DataFilters.MustHaveStore) == true;

        protected virtual string CurrentCityId => _coreSessionProvider?.Session?.City?.Id;

        protected virtual Guid? CurrentCompanyId => _coreSessionProvider?.Session?.Company?.Id;

        protected virtual Guid? CurrentStoreId => _coreSessionProvider?.Session?.Store?.Id;

        public virtual IMongoDatabase Database => _databaseResolver.Database;

        public virtual IMongoCollection<TEntity> Collection => Database.GetCollection<TEntity>(_collectionName);


        public override async ValueTask<TEntity> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await FirstOrDefaultAsync(id, cancellationToken);
            if (entity is null)
            {
                throw new EntityNotFoundException(typeof(TEntity), $"找不到键值为{id}的实体");
            }
            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                await UpdateAsync(entity, cancellationToken);
                return entity;
            }
            else
            {
                return await DeleteInternalAsync(id, cancellationToken);
            }
        }

        public override async ValueTask<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                await UpdateAsync(entity, cancellationToken);
                return entity;
            }
            else
            {
                return await DeleteInternalAsync(entity.Id, cancellationToken);
            }
        }

        public override IQueryable<TEntity> GetAll()
        {
            IQueryable<TEntity> query = Collection.AsQueryable();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsSoftDeleteFilterEnabled || !((ISoftDelete)e).IsDeleted);
            }

            if (typeof(IMayHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsMayHaveCityFilterEnabled || CurrentCityId == default || ((IMayHaveCity)e).CityId == CurrentCityId);
            }
            else if (typeof(IMustHaveCity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsMustHaveCityFilterEnabled || CurrentCityId == default || ((IMustHaveCity)e).CityId == CurrentCityId);
            }

            if (typeof(IMayHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsMayHaveCompanyFilterEnabled || CurrentCompanyId == default || ((IMayHaveCompany)e).CompanyId == CurrentCompanyId);
            }
            else if (typeof(IMustHaveCompany).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsMustHaveCompanyFilterEnabled || CurrentCompanyId == default || ((IMustHaveCompany)e).CompanyId == CurrentCompanyId);
            }

            if (typeof(IMayHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsMayHaveStoreFilterEnabled || CurrentCompanyId == default || ((IMayHaveStore)e).StoreId == CurrentStoreId);
            }
            else if (typeof(IMustHaveStore).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !IsMustHaveStoreFilterEnabled || CurrentCompanyId == default || ((IMustHaveStore)e).StoreId == CurrentStoreId);
            }

            return query;
        }

        public override ValueTask<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var session = _coreSessionProvider?.Session;
            CheckAndSetId(entity);
            entity.PerformConceptsOnAdding<TEntity, TKey>(session);
            entity.PerformAuditingOnAdding<TEntity, TKey>(session);
            return InsertInternalAsync(entity, cancellationToken);
        }

        private void CheckAndSetId(TEntity entity)
        {
            if (typeof(TKey) == typeof(Guid))
            {
                var entityWithGuidKey = (IEntity<Guid>)entity;
                if (_defaultValueEqualityDelegate(entity))
                {
                    entityWithGuidKey.Id = Guid.NewGuid();
                }
                if (GetAllInternal().Any(x => Equals(x.Id, entityWithGuidKey.Id)))
                {
                    throw new EntityKeyDuplicateException(typeof(TEntity), $"实体键{entity.Id}重复");
                }
            }
            else if (_defaultValueEqualityDelegate(entity))
            {
                throw new EntityKeyRequiredException(typeof(TEntity), $"缺少实体键");
            }
        }

        public override ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (_defaultValueEqualityDelegate(entity))
            {
                throw new EntityKeyRequiredException(typeof(TEntity), $"缺少实体键");
            }
            //处理一些审计自动字段..
            var session = _coreSessionProvider?.Session;
            entity.PerformAuditingOnUpdating<TEntity, TKey>(session);
            return UpdateInternalAsync(entity, cancellationToken);
        }

        #region Internal
        protected virtual IQueryable<TEntity> GetAllInternal() => Collection.AsQueryable();

        protected virtual async ValueTask<TEntity> InsertInternalAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Collection.InsertOneAsync(entity, null, cancellationToken);
            return entity;
        }
        protected virtual async ValueTask<TEntity> UpdateInternalAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Collection.FindOneAndReplaceAsync(CreateEqualityExpressionForId(entity.Id), entity, null, cancellationToken);
            return entity;
        }

        protected virtual async ValueTask<TEntity> DeleteInternalAsync(TKey id, CancellationToken cancellationToken)
        {
            return await Collection.FindOneAndDeleteAsync(CreateEqualityExpressionForId(id), null, cancellationToken);
        }

        #endregion Internal
    }
}
