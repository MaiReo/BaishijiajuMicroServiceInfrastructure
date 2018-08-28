using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Abstractions.PersistentStore.Entities
{
    public interface IMayHaveStoreName
    {
        string StoreName { get; set; }
    }
}
