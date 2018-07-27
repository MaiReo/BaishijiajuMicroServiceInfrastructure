using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.PersistentStore
{

    /// <summary>
    /// 实体
    /// </summary>
    /// <typeparam name="T">主键类型</typeparam>
    public abstract class Entity : Entity<int>, IEntity<int>, IEntity, IEntityBase
    {
    }

    /// <summary>
    /// 实体
    /// </summary>
    /// <typeparam name="T">主键类型</typeparam>
    public abstract class Entity<T> : IEntity<T>, IEntityBase
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual T Id { get; set; }
    }

    public abstract class EntityNoKey : EntityNoKey<Guid>, IEntityBase
    {
        protected EntityNoKey()
        {
        }

        protected EntityNoKey(Guid id) : base(id)
        {
        }
    }
    /// <summary>
    /// 不自动生成主键的实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EntityNoKey<T> : IEntity<T>, IEntityBase
    {
        protected EntityNoKey()
        {
        }

        protected EntityNoKey(T id)
        {
            this.Id = id;
        }

        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual T Id { get; set; }
    }
}
