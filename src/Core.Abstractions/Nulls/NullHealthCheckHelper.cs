using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ServiceDiscovery
{
    public class NullHealthCheckHelper : IHealthCheckHelper
    {
        public IEnumerable<IHealthCheckInfo> GetHeathCheckInfo()
        {
            return Enumerable.Empty<IHealthCheckInfo>();
        }
        public static NullHealthCheckHelper Instance => new NullHealthCheckHelper();
    }
}
