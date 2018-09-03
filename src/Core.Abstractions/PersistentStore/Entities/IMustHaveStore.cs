using System;

namespace Core.PersistentStore
{
    public interface IMustHaveStore
    {
        Guid StoreId { get; set; }

        string StoreName { get; set; }
    }
}
