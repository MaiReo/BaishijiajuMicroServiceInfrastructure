using System;
using System.Collections.Generic;
using System.Text;

namespace Core.PersistentStore
{
    public interface IMayHaveCompany
    {
        Guid? BrokerCompanyId { get; set; }
    }
}
