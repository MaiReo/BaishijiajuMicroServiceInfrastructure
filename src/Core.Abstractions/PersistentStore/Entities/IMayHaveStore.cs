using System;

namespace Core.PersistentStore
{
    public interface IMayHaveStore
    {
        Guid? StoreId { get; set; }

        string StoreName { get; set; }
    }
}
