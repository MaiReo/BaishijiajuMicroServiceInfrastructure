namespace Core.PersistentStore.Auditing
{
    public class FullAuditedEntity : FullAuditedEntity<int>, IFullAudited, IAudited, ICreationAudited, IHasCreationTime, IModificationAudited, IHasModificationTime, IDeletionAudited, IHasDeletionTime, ISoftDelete, IEntity<int>, IEntity, IEntityBase
    {

    }

    public class FullAuditedEntity<TKey> : AuditedEntity<TKey>, IFullAudited, IAudited, ICreationAudited, IHasCreationTime, IModificationAudited, IHasModificationTime, IDeletionAudited, IHasDeletionTime, ISoftDelete, IEntity<TKey>, IEntityBase
    {
        public virtual string DeleterUserId { get; set; }

        public virtual string DeleterUserName { get; set; }
    }
}
