namespace Core.PersistentStore.Auditing
{
    public class AuditedEntity : AuditedEntity<int>, IAudited, ICreationAudited, IHasCreationTime, IModificationAudited, IHasModificationTime, IHasDeletionTime, ISoftDelete, IEntity<int>, IEntity, IEntityBase
    {

    }

    public class AuditedEntity<TKey> : FullEntity<TKey>, IAudited, ICreationAudited, IHasCreationTime, IModificationAudited, IHasModificationTime, IHasDeletionTime, ISoftDelete, IEntity<TKey>, IEntityBase
    {
        public virtual string CreationUserId { get; set; }

        public virtual string CreationUserName { get; set; }

        public virtual string LastModifierUserId { get; set; }

        public virtual string LastModifierUserName { get; set; }
    }
}
