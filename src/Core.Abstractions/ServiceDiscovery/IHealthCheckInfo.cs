using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceDiscovery
{ 
    public interface IHealthCheckInfo
    {
        string Path { get; }
        int Interval { get; }
    }
}
