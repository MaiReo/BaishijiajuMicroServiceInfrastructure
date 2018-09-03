using System;
using System.Collections.Generic;
using System.Text;

namespace Core.PersistentStore
{
    public interface IMustHaveCompany
    {
        Guid CompanyId { get; set; }

        string CompanyName { get; set; }
    }
}
