using System;
using System.Collections.Generic;
using System.Text;

namespace Core.PersistentStore
{
    public abstract class FullEntity<T> : Entity<T>, IHasCreationTime, IHasModificationTime, ISoftDelete, IEntity<T>, IEntityBase
    {
        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset? LastModificationTime { get; set; }

        public bool IsDeleted { get; set; }
    }
}
