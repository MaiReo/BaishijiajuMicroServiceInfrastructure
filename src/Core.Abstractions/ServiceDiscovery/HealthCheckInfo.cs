using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceDiscovery
{
    public class HealthCheckInfo : IHealthCheckInfo
    {
        public HealthCheckInfo()
        {

        }
        public string Path { get; set; }
        public int Interval { get; set; }
    }
}
