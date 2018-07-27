using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceDiscovery
{
    public interface IHealthCheckHelper
    {
        IEnumerable<IHealthCheckInfo> GetHeathCheckInfo();
    }
}
