using Core.PersistentStore;
using Core.PersistentStore.Auditing;
using Core.Session;
using System;

namespace Core.DualCall.Extensions
{
    internal static class EntityAuditingExtensions
    {
        public static TEntity PerformConceptsOnAdding<TEntity,TKey>(this TEntity entity, ICoreSession session) where TEntity : IEntity<TKey>
        {
            var cityId = session?.City?.Id;
            var companyId = session?.Company?.Id;
            var companyName = session?.Company?.Name;
            var storeId = session?.Company?.Id;
            var storeName = session?.Company?.Name;
            var brokerId = session?.Broker?.Id;
            var brokerName = session?.Broker?.Name;
            if (entity is IMayHaveCity mayHaveCity)
            {
                if (string.IsNullOrWhiteSpace(mayHaveCity.CityId))
                {
                    mayHaveCity.CityId = cityId;
                }
            }
            else if (entity is IMustHaveCity mustHaveCity)
            {
                if (string.IsNullOrWhiteSpace(mustHaveCity.CityId))
                {
                    mustHaveCity.CityId = cityId;
                }
                if (string.IsNullOrWhiteSpace(mustHaveCity.CityId))
                {
                    throw new CityRequiredException(typeof(TEntity));
                }
            }
            if (entity is IMayHaveCompany mayHaveCompany)
            {
                if (mayHaveCompany.CompanyId.HasValue == false)
                {
                    mayHaveCompany.CompanyId = companyId;
                    if (string.IsNullOrWhiteSpace(mayHaveCompany.CompanyName))
                    {
                        mayHaveCompany.CompanyName = companyName;
                    }
                }
                
            }
            else if (entity is IMustHaveCompany mustHaveCompany)
            {
                if (Guid.Empty.Equals(mustHaveCompany.CompanyId))
                {
                    mustHaveCompany.CompanyId = companyId ?? throw new CompanyRequiredException(typeof(TEntity));
                    mustHaveCompany.CompanyName = companyName;
                }
                if (string.IsNullOrWhiteSpace(mustHaveCompany.CompanyName))
                {
                    throw new CompanyRequiredException(typeof(TEntity));
                }
            }

            if (entity is IMayHaveStore mayHaveStore)
            {
                if (mayHaveStore.StoreId.HasValue == false)
                {
                    mayHaveStore.StoreId = storeId;
                    if (string.IsNullOrWhiteSpace(mayHaveStore.StoreName))
                    {
                        mayHaveStore.StoreName = storeName;
                    }
                }

            }
            else if (entity is IMustHaveStore mustHaveStore)
            {
                if (Guid.Empty.Equals(mustHaveStore.StoreId))
                {
                    mustHaveStore.StoreId = storeId ?? throw new StoreRequiredException(typeof(TEntity));
                    mustHaveStore.StoreName = storeName;
                }
                if (string.IsNullOrWhiteSpace(mustHaveStore.StoreName))
                {
                    throw new StoreRequiredException(typeof(TEntity));
                }
            }

            if (entity is IMustHaveBroker mustHaveBroker)
            {
                if (string.IsNullOrWhiteSpace(mustHaveBroker.BrokerId))
                {
                    mustHaveBroker.BrokerId = brokerId ?? throw new BrokerRequiredException(typeof(TEntity));
                    mustHaveBroker.BrokerName = brokerName;
                }
                if (string.IsNullOrWhiteSpace(mustHaveBroker.BrokerName))
                {
                    throw new BrokerRequiredException(typeof(TEntity));
                }
            }

            return entity;

        }
        public static TEntity PerformAuditingOnAdding<TEntity,TKey>(this TEntity entity, ICoreSession session) where TEntity : IEntity<TKey>
        {
            string cityId = session?.City?.Id;
            string currentUserId = session?.User?.Id;
            string currentUserName = session?.User?.Name;
            if (entity is IHasCreationTime hasCreationTime)
            {
                hasCreationTime.CreationTime = Clock.Now;
            }
            if (entity is IHasModificationTime hasModificationTime)
            {
                hasModificationTime.LastModificationTime = null;
            }
            if (entity is ICreationAudited audited)
            {
                if (string.IsNullOrWhiteSpace(audited.CreationUserId))
                {
                    audited.CreationUserId = currentUserId;
                }
                if (string.IsNullOrWhiteSpace(audited.CreationUserName))
                {
                    audited.CreationUserName = currentUserName;
                }
            }
            return entity;
        }

        public static TEntity PerformAuditingOnUpdating<TEntity, TKey>(this TEntity entity, ICoreSession session) where TEntity : IEntity<TKey>
        {
            string cityId = session?.City?.Id;
            string currentUserId = session?.User?.Id;
            string currentUserName = session?.User?.Name;
            if (entity is IHasModificationTime hasModificationTime)
            {
                hasModificationTime.LastModificationTime = Clock.Now;
            }

            if (entity is IModificationAudited audited)
            {
                if (string.IsNullOrWhiteSpace(audited.LastModifierUserId))
                {
                    audited.LastModifierUserId = currentUserId;
                }
                if (string.IsNullOrWhiteSpace(audited.LastModifierUserName))
                {
                    audited.LastModifierUserName = currentUserName;
                }
            }
            if (entity is ISoftDelete softDelete && softDelete.IsDeleted)
            {
                if (entity is IHasDeletionTime hasDeletionTime)
                {
                    hasDeletionTime.DeletionTime = Clock.Now;
                }
                if (entity is IDeletionAudited deletionAudited)
                {
                    if (string.IsNullOrWhiteSpace(deletionAudited.DeleterUserId))
                    {
                        deletionAudited.DeleterUserId = currentUserId;
                    }
                    if (string.IsNullOrWhiteSpace(deletionAudited.DeleterUserName))
                    {
                        deletionAudited.DeleterUserName = currentUserName;
                    }
                }
            }
            return entity;
        }


        public static TEntity PerformAuditingOnDeleting<TEntity, TKey>(this TEntity entity, ICoreSession session) where TEntity : IEntity<TKey>
        {
            string cityId = session?.City?.Id;
            string currentUserId = session?.User?.Id;
            string currentUserName = session?.User?.Name;
            if (entity is IHasModificationTime hasModificationTime)
            {
                hasModificationTime.LastModificationTime = Clock.Now;
            }

            if (entity is IModificationAudited audited)
            {
                if (string.IsNullOrWhiteSpace(audited.LastModifierUserId))
                {
                    audited.LastModifierUserId = currentUserId;
                }
                if (string.IsNullOrWhiteSpace(audited.LastModifierUserName))
                {
                    audited.LastModifierUserName = currentUserName;
                }
            }
            if (entity is ISoftDelete softDelete && softDelete.IsDeleted)
            {
                if (entity is IHasDeletionTime hasDeletionTime)
                {
                    hasDeletionTime.DeletionTime = Clock.Now;
                }
                if (entity is IDeletionAudited deletionAudited)
                {
                    if (string.IsNullOrWhiteSpace(deletionAudited.DeleterUserId))
                    {
                        deletionAudited.DeleterUserId = currentUserId;
                    }
                    if (string.IsNullOrWhiteSpace(deletionAudited.DeleterUserName))
                    {
                        deletionAudited.DeleterUserName = currentUserName;
                    }
                }
            }
            return entity;
        }
    }
}
